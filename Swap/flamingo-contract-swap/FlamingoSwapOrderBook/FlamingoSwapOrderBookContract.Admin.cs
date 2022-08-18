using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace FlamingoSwapOrderBook
{
    public partial class FlamingoSwapOrderBookContract
    {
        #region Admin

#warning Update the admin address if necessary
        //Test
        //[InitialValue("NVGUQ1qyL4SdSm7sVmGVkXetjEsvw2L3NT", ContractParameterType.Hash160)]
        //static readonly UInt160 superAdmin = default;
        //[InitialValue("0x48c40d4666f93408be1bef038b6722404d9a4c2a", ContractParameterType.Hash160)]
        //static readonly UInt160 bNEO = default;

        //Main
        [InitialValue("NdDvLrbtqeCVQkaLstAwh3md8SYYwqWRaE", ContractParameterType.Hash160)]
        static readonly UInt160 superAdmin = default;
        [InitialValue("0xfb75a5314069b56e136713d38477f647a13991b4", Neo.SmartContract.ContractParameterType.Hash160)]
        static readonly UInt160 WhiteListContract = default;
        [InitialValue("0x48c40d4666f93408be1bef038b6722404d9a4c2a", ContractParameterType.Hash160)]
        static readonly UInt160 bNEO = default;

        private const string AdminKey = nameof(superAdmin);
        private const string GASAdminKey = nameof(GASAdminKey);
        private const string FundAddresskey = nameof(FundAddresskey);
        private const string WhiteListContractKey = nameof(WhiteListContract);

        private static readonly byte[] OrderIDKey = new byte[] { 0x00 };
        private static readonly byte[] BookMapPrefix = new byte[] { 0x01 };
        private static readonly byte[] OrderMapPrefix = new byte[] { 0x02 };
        private static readonly byte[] ReceiptMapPrefix = new byte[] { 0x03 };
        private static readonly byte[] PauseMapPrefix = new byte[] { 0x04 };

        // When this contract address is included in the transaction signature,
        // this method will be triggered as a VerificationTrigger to verify that the signature is correct.
        // For example, this method needs to be called when withdrawing token from the contract.
        [Safe]
        public static bool Verify() => Runtime.CheckWitness(GetAdmin());

        [Safe]
        public static UInt160 GetAdmin()
        {
            var admin = StorageGet(AdminKey);
            return admin?.Length == 20 ? (UInt160)admin : superAdmin;
        }

        public static bool SetAdmin(UInt160 admin)
        {
            Assert(Verify(), "No Authorization");
            Assert(admin.IsAddress(), "Invalid Address");
            StoragePut(AdminKey, admin);
            return true;
        }

        public static void ClaimGASFrombNEO(UInt160 receiveAddress)
        {
            Assert(Runtime.CheckWitness(GetGASAdmin()), "Forbidden");
            var me = Runtime.ExecutingScriptHash;
            var beforeBalance = GAS.BalanceOf(me);
            Assert((bool)Contract.Call(bNEO, "transfer", CallFlags.All, Runtime.ExecutingScriptHash, bNEO, 0, null), "claim fail");
            var afterBalance = GAS.BalanceOf(me);

            GAS.Transfer(me, receiveAddress, afterBalance - beforeBalance);
        }

        [Safe]
        public static UInt160 GetGASAdmin()
        {
            var address = StorageGet(GASAdminKey);
            return address?.Length == 20 ? (UInt160)address : null;
        }

        public static bool SetGASAdmin(UInt160 GASAdmin)
        {
            Assert(GASAdmin.IsAddress(), "Invalid Address");
            Assert(Verify(), "No Authorization");
            StoragePut(GASAdminKey, GASAdmin);
            return true;
        }
        #endregion

        #region WhiteContract

        [Safe]
        public static UInt160 GetWhiteListContract()
        {
            var whiteList = StorageGet(WhiteListContractKey);
            return whiteList?.Length == 20 ? (UInt160)whiteList : WhiteListContract;
        }

        public static bool SetWhiteListContract(UInt160 whiteList)
        {
            Assert(Verify(), "Forbidden");
            Assert(whiteList.IsAddress(), "Invalid Address");
            StoragePut(WhiteListContractKey, whiteList);
            return true;
        }

        public static bool CheckIsRouter(UInt160 callScript)
        {
            Assert(callScript.IsAddress(), "Invalid CallScript Address");
            var whiteList = GetWhiteListContract();
            return (bool)Contract.Call(whiteList, "checkRouter", CallFlags.ReadOnly, new object[] { callScript });
        }

        #endregion

        #region FundFee

        [Safe]
        public static UInt160 GetFundAddress()
        {
            var address = StorageGet(FundAddresskey);
            return address?.Length == 20 ? (UInt160)address : null;
        }

        public static bool SetFundAddress(UInt160 address)
        {
            Assert(address.IsAddress(), "Invalid Address");
            Assert(Verify(), "No Authorization");
            StoragePut(FundAddresskey, address);
            return true;
        }
        #endregion

        #region Upgrade

        public static void Update(ByteString nefFile, string manifest)
        {
            Assert(Verify(), "No Authorization");
            ContractManagement.Update(nefFile, manifest, null);
        }
        #endregion
    }
}
