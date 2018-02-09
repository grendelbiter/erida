using Assets.Wulfram3.Scripts.InternalApis.Classes;
using System;
using System.Threading.Tasks;

namespace Assets.Wulfram3.Scripts.InternalApis.Interfaces
{
    public interface IUserController
    {
        void UpdateUserData();

        void RecordUnitKill(UnitType type);

        void RecordUnitDeploy(UnitType type);

        void RecordPlayerDeath(UnitType type);

        WulframPlayer GetWulframPlayerData();

        Task<WulframPlayer> LoginUser(string username, string password);

        Task<string> RegisterUser(string username, string password, string email);

    }
}