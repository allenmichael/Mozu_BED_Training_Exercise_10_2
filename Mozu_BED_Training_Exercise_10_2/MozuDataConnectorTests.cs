using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mozu.Api;
using Autofac;
using Mozu.Api.ToolKit.Config;
using System.Collections.Generic;

namespace Mozu_BED_Training_Exercise_10_2
{
    [TestClass]
    public class MozuDataConnectorTests
    {
        private IApiContext _apiContext;
        private IContainer _container;

        [TestInitialize]
        public void Init()
        {
            _container = new Bootstrapper().Bootstrap().Container;
            var appSetting = _container.Resolve<IAppSetting>();
            var tenantId = int.Parse(appSetting.Settings["TenantId"].ToString());
            var siteId = int.Parse(appSetting.Settings["SiteId"].ToString());

            _apiContext = new ApiContext(tenantId, siteId);
        }

        [TestMethod]
        public void Exercise_10_1_Get_Product_Type()
        {
            //Create a new ProductType resource
            var productTypeResource = new Mozu.Api.Resources.Commerce.Catalog.Admin.Attributedefinition.ProductTypeResource(_apiContext);

            //Now that you're more familiar with using the Mozu API documentation, I'll just give you the link for the ProductType JSON information
            //http://developer.mozu.com/content/api/APIResources/commerce/commerce.catalog/commerce.catalog.admin.attributedefinition.producttypes.htm

            //Add Your Code: 
            //Get list of attributes with max page size and starting index at the beginning
            var productTypes = productTypeResource.GetProductTypesAsync(startIndex: 0, pageSize: 200).Result;

            //Add Your Code: 
            //Write total count of producttypes to output window
            System.Diagnostics.Debug.WriteLine(string.Format("Product Type Total Count: {0}", productTypes.TotalCount));

            //Add Your Code: 
            //Get product type filtered by name 
            var typePurse = productTypeResource.GetProductTypesAsync(filter: "name sw 'Purse'").Result.Items[0];

            //Loop through List<AttributeInProductType>
            foreach (var option in typePurse.Options)
            {
                //Loop through List<AttributeVocabularyValueInProductType>
                foreach (var value in option.VocabularyValues)
                {
                    //Add Your Code: 
                    //Write vocabulary values to output window
                    System.Diagnostics.Debug.WriteLine(string.Format("Product Type [{0}]: Value({1}) StringValue({2})",
                        typePurse.Name, value.Value, value.VocabularyValueDetail.Content.StringValue));
                }
            }
        }

        [TestMethod]
        public void Exercise_10_2_Update_ProductTypes()
        {
            #region Add "Monogram" as an Extra
            //Create a new ProductType resource
            var producttypeResource = new Mozu.Api.Resources.Commerce.Catalog.Admin.Attributedefinition.ProductTypeResource(_apiContext);

            //Add Your Code: 
            //Get product type filtered by name 
            var typePurse = (producttypeResource.GetProductTypesAsync(filter: "name sw 'purse'").Result).Items[0];

            //Create an Attribute resource
            var productAttributeResource = new Mozu.Api.Resources.Commerce.Catalog.Admin.Attributedefinition.AttributeResource(_apiContext);

            //In case you've forgotten about Product Attributes:
            //http://developer.mozu.com/content/api/APIResources/commerce/commerce.catalog/commerce.catalog.admin.attributedefinition.attributes.htm

            //Add Your Code: 
            //Get a monogram attribute
            var attrMonogram = productAttributeResource.GetAttributeAsync("tenant~monogram").Result;

            //Add Your Code: 
            //Create new object defining new extra
            var attributeInProductTypeMonogram = new Mozu.Api.Contracts.ProductAdmin.AttributeInProductType()
            {
                AttributeDetail = attrMonogram,
                AttributeFQN = attrMonogram.AttributeFQN,
                
            };

            //Test if the Extra already exists
            if (typePurse.Extras.Find(x => x.AttributeFQN == "tenant~monogram") == null)
            {
                //Add Your Code: 
                //Update product type with new extra
                typePurse.Extras.Add(attributeInProductTypeMonogram);

                //Update product type
                var updatedTypeMonogram = producttypeResource.UpdateProductTypeAsync(typePurse, (int)typePurse.Id).Result;
            }
            #endregion

            #region Add "Purse-Size" as an Option
            //Add Your Code: 
            //Get a purse size attribute
            var attrSize = productAttributeResource.GetAttributeAsync("tenant~purse-size").Result;

            //Add Your Code: 
            //Create new object defining new option
            var attributeInProductTypePurse = new Mozu.Api.Contracts.ProductAdmin.AttributeInProductType()
            {
                AttributeDetail = attrSize,
                AttributeFQN = attrSize.AttributeFQN,
                VocabularyValues = new List<Mozu.Api.Contracts.ProductAdmin.AttributeVocabularyValueInProductType>(),
                Order = 0
            };

            //Add Your Code: 
            //Exclude "Alta" from sizes
            var doNotInclude = "Alta";

            //A variable needed for Option types to providea hierarchical order for each value
            var sortOrder = 0;

            foreach (var value in attrSize.VocabularyValues)
            {
                if (!doNotInclude.ToLower().Contains(value.Content.StringValue.ToLower()))
                {
                    attributeInProductTypePurse.VocabularyValues.Add(new Mozu.Api.Contracts.ProductAdmin.AttributeVocabularyValueInProductType 
                    {
                        Value = value.Value,
                        VocabularyValueDetail = value,
                        Order = sortOrder++
                    });
                }
            }

            //Test if the Option already exists
            if (typePurse.Options.Find(x => x.AttributeFQN == "tenant~purse-size") == null)
            {
                //update product type with new option
                typePurse.Options.Add(attributeInProductTypePurse);

                //update product type
                var updatedTypePurse = producttypeResource.UpdateProductTypeAsync(typePurse, (int)typePurse.Id).Result;
            }
            #endregion
        }
    }
}
