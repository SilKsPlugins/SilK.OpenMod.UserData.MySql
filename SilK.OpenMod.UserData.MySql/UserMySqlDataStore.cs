extern alias JetBrainsAnnotations;
using Autofac;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using SilK.OpenMod.UserData.MySql.Database;
using SilK.OpenMod.UserData.MySql.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SilK.OpenMod.UserData.MySql
{
    [UsedImplicitly]
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Normal)]
    public class UserMySqlDataStore : IUserDataStore
    {
        private readonly IPluginAccessor<OpenModUserDataMySqlPlugin> _pluginAccessor;

        public UserMySqlDataStore(IPluginAccessor<OpenModUserDataMySqlPlugin> pluginAccessor)
        {
            _pluginAccessor = pluginAccessor;
        }

        private UserDataDbContext GetDbContext()
        {
            var plugin = _pluginAccessor.Instance ?? throw new InvalidOperationException("Plugin not loaded");

            return plugin.LifetimeScope.Resolve<UserDataDbContext>();
        }

        private IQueryable<UserDataModel> GetUsersIncludeAll(UserDataDbContext dbContext)
        {
            return dbContext.Users
                .Include(x => x.BanInfo)
                .Include(x => x.Permissions)
                .Include(x => x.Roles)
                .Include(x => x.Data);
        }

        private global::OpenMod.API.Users.UserData GetUserDataFromModel(UserDataModel model)
        {
            return new()
            {
                Id = model.Id,
                Type = model.Type,
                LastDisplayName = model.LastDisplayName,
                //FirstSeen = model.FirstSeen == long.MinValue ? null : new DateTime(model.FirstSeen),
                FirstSeen = model.FirstSeen,
                //LastSeen = model.LastSeen == long.MinValue ? null : new DateTime(model.LastSeen),
                LastSeen = model.LastSeen,
                BanInfo = model.BanInfo == null
                    ? null
                    : new()
                    {
                        /*ExpireDate = model.BanInfo.ExpireDate == long.MinValue
                            ? null
                            : new DateTime(model.BanInfo.ExpireDate),*/
                        ExpireDate = model.BanInfo.ExpireDate,
                        InstigatorId = model.BanInfo.InstigatorId,
                        InstigatorType = model.BanInfo.InstigatorType,
                        Reason = model.BanInfo.Reason
                    },
                Permissions = new HashSet<string>(model.Permissions.Select(x => x.Permission)),
                Roles = new HashSet<string>(model.Roles.Select(x => x.Role)),
                Data = model.Data.ToDictionary(x => x.Key,
                    x => x.Object == null ? null : JsonConvert.DeserializeObject(x.Object))
            };
        }

        public async Task<global::OpenMod.API.Users.UserData?> GetUserDataAsync(string userId, string userType)
        {
            await using var dbContext = GetDbContext();

            var userDataModel = await GetUsersIncludeAll(dbContext)
                .FirstOrDefaultAsync(x => x.Id == userId && x.Type == userType);

            return userDataModel == null ? null : GetUserDataFromModel(userDataModel);
        }

        public async Task<T?> GetUserDataAsync<T>(string userId, string userType, string key)
        {
            await using var dbContext = GetDbContext();

            var dataObject = await dbContext.UserDataObjects.FindAsync(userId, userType, key);

            return dataObject?.Object == null ? default : JsonSerializer.Deserialize<T>(dataObject.Object);
        }

        public async Task<IReadOnlyCollection<global::OpenMod.API.Users.UserData>> GetUsersDataAsync(string type)
        {
            await using var dbContext = GetDbContext();

            var userDataModels = await GetUsersIncludeAll(dbContext)
                .Where(x => x.Type == type)
                .ToListAsync();

            return userDataModels.Select(GetUserDataFromModel).ToList();
        }

        public async Task SetUserDataAsync(global::OpenMod.API.Users.UserData userData)
        {
            if (userData.Id == null)
            {
                throw new ArgumentNullException(nameof(userData.Id), "User data ID cannot be null");
            }

            if (userData.Type == null)
            {
                throw new ArgumentNullException(nameof(userData.Id), "User data Type cannot be null");
            }

            await using var dbContext = GetDbContext();

            var userModel = await GetUsersIncludeAll(dbContext)
                .FirstOrDefaultAsync(x => x.Id == userData.Id && x.Type == userData.Type);

            void SetKeys(UserDataKeyBase keyBase)
            {
                keyBase.Id = userData!.Id;
                keyBase.Type = userData!.Type;
            }

            async Task AddUpdateRemove<TModel, TData>(TModel? oldModel, TData? newData,
                Action<TModel, TData> update) where TModel : class, new()
            {
                var set = dbContext.Set<TModel>();

                if (oldModel == null)
                {
                    if (newData == null)
                    {
                        return;
                    }

                    // Add

                    var newModel = new TModel();

                    update(newModel, newData);

                    if (newModel is UserDataKeyBase keyBase)
                    {
                        SetKeys(keyBase);
                    }

                    await set.AddAsync(newModel);
                }
                else
                {
                    if (newData == null)
                    {
                        // Remove

                        set.Remove(oldModel);
                    }
                    else
                    {
                        // Update

                        update(oldModel, newData);

                        set.Update(oldModel);
                    }
                }
            }

            async Task AddUpdateRemoveMultiple<TModel, TData>(
                ICollection<TModel>? oldModel, ICollection<TData>? newData,
                Func<TModel, TData, bool> modelDataMatch,
                Action<TModel, TData> update) where TModel : class, new()
            {
                var set = dbContext.Set<TModel>();

                oldModel ??= Array.Empty<TModel>();
                newData ??= Array.Empty<TData>();

                var toBeUpdated = new List<(TModel Model, TData Data)>();
                var toBeRemoved = new List<TModel>();

                foreach (var model in oldModel)
                {
                    var data = newData.FirstOrDefault(x => modelDataMatch(model, x));

                    if (data != null)
                    {
                        toBeUpdated.Add((model, data));
                    }
                    else
                    {
                        toBeRemoved.Add(model);
                    }
                }

                var toBeAdded = newData.Except(toBeUpdated.Select(x => x.Data)).ToList();

                foreach (var model in toBeRemoved)
                {
                    set.Remove(model);
                }

                foreach (var (model, data) in toBeUpdated)
                {
                    update(model, data);

                    set.Update(model);
                }

                foreach (var data in toBeAdded)
                {
                    var newModel = new TModel();

                    update(newModel, data);

                    if (newModel is UserDataKeyBase keyBase)
                    {
                        SetKeys(keyBase);
                    }

                    await set.AddAsync(newModel);
                }
            }

            await AddUpdateRemove(userModel, userData,
                (model, data) =>
                {
                    model.FirstSeen = data.FirstSeen;
                    model.LastSeen = data.LastSeen;
                    model.LastDisplayName = data.LastDisplayName;
                });

            await AddUpdateRemove(userModel?.BanInfo, userData.BanInfo,
                (model, data) =>
                {
                    model.ExpireDate = data.ExpireDate;
                    model.InstigatorId = data.InstigatorId;
                    model.InstigatorType = data.InstigatorType;
                    model.Reason = data.Reason;
                });

            await AddUpdateRemoveMultiple(userModel?.Permissions, userData?.Permissions,
                (model, data) => model.Permission.Equals(data),
                (model, data) =>
                {
                    model.Permission = data;
                });

            await AddUpdateRemoveMultiple(userModel?.Roles, userData?.Roles,
                (model, data) => model.Role.Equals(data),
                (model, data) =>
                {
                    model.Role = data;
                });

            await AddUpdateRemoveMultiple(userModel?.Data, userData?.Data,
                (model, data) => model.Key.Equals(data.Key),
                (model, data) =>
                {
                    model.Key = data.Key;
                    model.Object = JsonSerializer.Serialize(data.Value);
                });

            await dbContext.SaveChangesAsync();
        }

        public async Task SetUserDataAsync<T>(string userId, string userType, string key, T? value)
        {
            await using var dbContext = GetDbContext();

            var dataObject = await dbContext.UserDataObjects.FindAsync(userId, userType, key);

            var json = value == null ? null : JsonSerializer.Serialize(value);

            if (dataObject == null)
            {
                dataObject = new UserDataObjectModel
                {
                    Id = userId,
                    Type = userType,
                    Key = key,
                    Object = json
                };

                await dbContext.UserDataObjects.AddAsync(dataObject);
            }
            else
            {
                dataObject.Object = json;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
