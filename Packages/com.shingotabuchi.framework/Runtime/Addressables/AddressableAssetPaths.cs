namespace Fwk.Addressables
{
    public static class AddressableAssetKeys
    {
        private const string Root = "Assets/AddressableResources/";
        private const string CueSheets = Root + "Sounds/CueSheets/";

        public static string GetCueSheetKey(string cueSheetName)
        {
            return $"{CueSheets}{cueSheetName}.asset";
        }
    }
}
