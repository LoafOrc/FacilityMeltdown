using Biodiversity.Util.Assetloading;
using FacilityMeltdown.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace FacilityMeltdown.Util;
internal abstract class AssetBundleLoader<T> where T : AssetBundleLoader<T> {

    public AssetBundleLoader(string filePath) {
        AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filePath));
        MeltdownPlugin.logger.LogDebug($"[Assets] AssetBundle `{filePath}` contains these objects: \n{string.Join("\n", bundle.GetAllAssetNames())}");

        Type type = typeof(T);
        foreach(PropertyInfo field in type.GetProperties()) {
            LoadFromBundleAttribute loadInstruction = (LoadFromBundleAttribute)field.GetCustomAttribute(typeof(LoadFromBundleAttribute));
            if(loadInstruction == null) continue;

            MeltdownPlugin.logger.LogDebug($"[Assets] Got LoadFromBundle attribute on `{field.Name}`. Loading asset: `{loadInstruction.BundleFile}` into the [roperty.");
            field.SetValue(this, LoadAsset<UnityEngine.Object>(bundle, loadInstruction.BundleFile.ToLower()));
        }

        foreach(GameObject gameObject in bundle.LoadAllAssets<GameObject>()) {
            if(gameObject.GetComponent<NetworkObject>() == null) continue;
            if(GameNetworkManagerPatch.networkPrefabsToRegister.Contains(gameObject)) continue;
            GameNetworkManagerPatch.networkPrefabsToRegister.Add(gameObject);
        }

        FinishLoadingAssets(bundle);
        //bundle.Unload(false);
    }

    protected virtual void FinishLoadingAssets(AssetBundle bundle) { }

    protected AssetType LoadAsset<AssetType>(AssetBundle bundle, string path) where AssetType : UnityEngine.Object {
        AssetType result = bundle.LoadAsset<AssetType>(path);
        if(result == null) throw new ArgumentException(path + " is not valid in the assetbundle!");

        return result;
    }
}
