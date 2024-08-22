using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;

namespace FlamingoSwapPair
{
    partial class FlamingoSwapPairContract
    {
        #region Settings

        //Test
        static readonly UInt160 superAdmin = "NVGUQ1qyL4SdSm7sVmGVkXetjEsvw2L3NT";

        static readonly UInt160 WhiteListContract = "0xf9c55e595b39ded1e866efcc163445a168d378d5";

        ////Main
        //[InitialValue("NdDvLrbtqeCVQkaLstAwh3md8SYYwqWRaE", Neo.SmartContract.ContractParameterType.Hash160)]
        //static readonly UInt160 superAdmin = default;

        //[InitialValue("0xfb75a5314069b56e136713d38477f647a13991b4", Neo.SmartContract.ContractParameterType.Hash160)]
        //static readonly UInt160 WhiteListContract = default;

        #region TokenAB


        [Safe]
        public static string Symbol() => "FLP-fWBTC-fUSDT"; //symbol of the token

        #region tokenA 
        //bENO
        static readonly UInt160 TokenA = "0x9b9f7db02c0a2d6aa5ad9be7bba843027bf5b5f2";
        #endregion

        static readonly UInt160 TokenB = "0xa2d5f4378c42d118ebc7e1690f9478d3e00aefa1";


        #endregion

        #endregion

        #region Admin


        const string AdminKey = nameof(superAdmin);
        const string GASAdminKey = nameof(GASAdminKey);
        const string FundAddresskey = nameof(FundAddresskey);
        private const string WhiteListContractKey = nameof(WhiteListContract);

        // When this contract address is included in the transaction signature,
        // this method will be triggered as a VerificationTrigger to verify that the signature is correct.
        // For example, this method needs to be called when withdrawing token from the contract.
        public static bool Verify() => Runtime.CheckWitness(GetAdmin());



        /// <summary>
        /// 获取合约管理员
        /// </summary>
        /// <returns></returns>
        public static UInt160 GetAdmin()
        {
            var admin = StorageGet(AdminKey);
            return admin?.Length == 20 ? (UInt160)admin : superAdmin;
        }

        /// <summary>
        /// 设置合约管理员
        /// </summary>
        /// <param name="admin"></param>
        /// <returns></returns>
        public static bool SetAdmin(UInt160 admin)
        {
            Assert(admin.IsAddress(), "Invalid Address");
            Assert(Runtime.CheckWitness(GetAdmin()), "Forbidden");
            StoragePut(AdminKey, admin);
            return true;
        }

        public static void ClaimGASFrombNEO(UInt160 receiveAddress)
        {
            Assert(Runtime.CheckWitness(GetGASAdmin()), "Forbidden");
            var me = Runtime.ExecutingScriptHash;
            BigInteger beforeBalance = GAS.BalanceOf(me);
            Assert((bool)Contract.Call(TokenA, "transfer", CallFlags.All, Runtime.ExecutingScriptHash, TokenA, 0, null), "claim fail");
            BigInteger afterBalance = GAS.BalanceOf(me);

            GAS.Transfer(me, receiveAddress, afterBalance - beforeBalance);
        }

        public static UInt160 GetGASAdmin()
        {
            var admin = StorageGet(GASAdminKey);
            return (UInt160)admin;
        }

        public static bool SetGASAdmin(UInt160 GASAdmin)
        {
            Assert(GASAdmin.IsAddress(), "Invalid Address");
            Assert(Runtime.CheckWitness(GetAdmin()), "Forbidden");
            StoragePut(GASAdminKey, GASAdmin);
            return true;
        }

        #endregion

        #region WhiteContract


        /// <summary>
        /// 获取WhiteListContract地址
        /// </summary>
        /// <returns></returns>
        public static UInt160 GetWhiteListContract()
        {
            var whiteList = StorageGet(WhiteListContractKey);
            return whiteList?.Length == 20 ? (UInt160)whiteList : WhiteListContract;
        }

        /// <summary>
        /// 设置WhiteListContract地址
        /// </summary>
        /// <param name="whiteList"></param>
        /// <returns></returns>
        public static bool SetWhiteListContract(UInt160 whiteList)
        {
            Assert(Runtime.CheckWitness(GetAdmin()), "Forbidden");
            Assert(whiteList.IsAddress(), "Invalid Address");
            StoragePut(WhiteListContractKey, whiteList);
            return true;
        }

        /// <summary>
        /// 检查<see cref="callScript"/>是否为router合约
        /// </summary>
        /// <param name="callScript"></param>
        /// <returns></returns>
        public static bool CheckIsRouter(UInt160 callScript)
        {
            Assert(callScript.IsAddress(), "Invalid CallScript Address");
            var whiteList = GetWhiteListContract();
            return (bool)Contract.Call(whiteList, "checkRouter", CallFlags.ReadOnly, new object[] { callScript });
        }

        #endregion

        #region FundFee

        /// <summary>
        /// 获取FundAddress
        /// </summary>
        /// <returns></returns>
        public static UInt160 GetFundAddress()
        {
            var address = StorageGet(FundAddresskey);
            return address?.Length == 20 ? (UInt160)address : null;
        }

        /// <summary>
        /// 设置FundAddress
        /// </summary>
        /// <param name="admin"></param>
        /// <returns></returns>
        public static bool SetFundAddress(UInt160 address)
        {
            Assert(address.IsAddress(), "Invalid Address");
            Assert(Runtime.CheckWitness(GetAdmin()), "Forbidden");
            StoragePut(FundAddresskey, address);
            return true;
        }
        #endregion

        #region Upgrade


        /// <summary>
        /// 升级
        /// </summary>
        /// <param name="nefFile"></param>
        /// <param name="manifest"></param>
        /// <param name="data"></param>
        public static void Update(ByteString nefFile, string manifest)
        {
            Assert(Verify(), "No authorization.");
            ContractManagement.Update(nefFile, manifest, null);
        }

        #endregion
    }
}
