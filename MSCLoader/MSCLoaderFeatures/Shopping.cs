using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSCLoader.Shopping
{
    internal class FleetariShopCore : MonoBehaviour
    {
        static FleetariShopCore instance;

        public List<FleetariProduct> products;
        GameObject shop;

        RaycastHit hitInfo;
        Camera playerCamera;
        Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f);
        LayerMask productLayerMask;

        bool mouseOver = false;

        public static FleetariShopCore Setup()
        {

        }

        void Awake()
        {
            hitInfo = new RaycastHit();
            playerCamera = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/FPSCamera").GetComponent<Camera>();
            productLayerMask = 1 << LayerMask.NameToLayer("DontCollide");
        }

        public void Update()
        {
            if (shop.activeSelf && Physics.Raycast(playerCamera.ViewportPointToRay(viewportCenter), out hitInfo, 1.5f, productLayerMask) && hitInfo.collider.transform)
            {
                StoreProduct product = hitInfo.collider.GetComponent<StoreProduct>();
                if (product != null)
                {
                    mouseOver = true;
                    PlayMakerHelper.GUIBuy = true;
                    PlayMakerHelper.GUIInteraction = $" {product.productName}, {product.products} mk ";

                    if (cInput.GetButtonDown("Use") || Input.GetMouseButtonDown(0))

                }
                else if (mouseOver) MouseOver();
            }
            else if (mouseOver) MouseOver();
        }

        void BuyProduct(StoreProduct product)
        {

        }

        void MouseOver()
        {

        }
    }

    public class FleetariProduct : MonoBehaviour
    {

        [Header("Shopping System, created by Fredrik!"), Space(10)]
        public float price = 0f;
        public string productName = "";
        public int amount = 1;

        public GameObject productPrefab;

        [HideInInspector]
        public int amountOrdered;

        public Renderer[] products = new Renderer[1];

        public bool restocks = false;

        void Start()
        {
            FleetariShopCore.Setup().products.Add(this);
        }

        public void Restock()
        {
            for (int i = 0; i < products.Length; i++)
                products[i].enabled = true;
        }

    }
}
