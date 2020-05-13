using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveMaterial.Utils
{
    /// <summary>
    /// Utilities to find objects specific to Beat Saber
    /// </summary>
    internal static class BeatSaberSearching
    {


        /// <summary>
        /// Provides an invariant format provider, without having to create a variable for it in every file
        /// </summary>
        internal const StringComparison STR_INV = StringComparison.Ordinal;

        /// <summary>
        /// Finds the current environment <see cref="Scene"/>, prioritizing non-menu environments
        /// </summary>
        /// <exception cref="EnvironmentSceneNotFoundException"></exception>
        /// <returns>The current environment <see cref="Scene"/></returns>
        internal static Scene GetCurrentEnvironment()
        {
            Scene scene = new Scene();
            Scene environmentScene = scene;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);
                if (scene.name.EndsWith("Environment", STR_INV) && (!environmentScene.IsValid() || environmentScene.name.StartsWith("Menu", STR_INV)))
                {
                    environmentScene = scene;
                }
            }
            if (environmentScene.IsValid())
            {
                return environmentScene;
            }
            throw new EnvironmentSceneNotFoundException();
        }



        /// <summary>
        /// Finds the <see cref="LightWithIdManager"/> of the given <paramref name="environment"/>
        /// </summary>
        /// <param name="environment">Where to search for a <see cref="LightWithIdManager"/></param>
        /// <exception cref="ManagerNotFoundException"></exception>
        internal static LightWithIdManager FindLightWithIdManager(Scene environment)
        {
            LightWithIdManager manager = null;
            void RecursiveFindManager(Transform directParent)
            {
                for (int i = 0; i < directParent.childCount; i++)
                {
                    Transform child = directParent.GetChild(i);
                    if (child.GetComponent<LightWithIdManager>() != null)
                    {
                        manager = child.GetComponent<LightWithIdManager>();
                    }
                    if (child.childCount != 0)
                    {
                        RecursiveFindManager(child);
                    }
                }
            }
            GameObject[] roots = environment.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                RecursiveFindManager(root.transform);
            }
            if (manager != null)
            {
                return manager;
            }
            else
            {
                throw new ManagerNotFoundException();
            }
        }

        public sealed class EnvironmentSceneNotFoundException : Exception
        {
            internal EnvironmentSceneNotFoundException() :
                base("No Environment Scene could be found!")
            {

            }
        }

        /// <summary>
        /// The manager class you were looking for does not appear to be instantiated right now
        /// </summary>
        public class ManagerNotFoundException : Exception
        {
            internal ManagerNotFoundException() :
                base("No such Manager could be found!")
            {

            }
            internal ManagerNotFoundException(Exception e) :
                base("No such Manager could be found!", e)
            {

            }
        }
    }
}
