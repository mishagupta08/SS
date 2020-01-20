using InventoryManagement.API.Controllers;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.DataAccess
{
    public class OfferRepository
    {
        OfferAPIController objOfferAPI = new OfferAPIController();
        public List<Offer> GetValidOfferList(string Doj, string UpgradeDate, string IsFirstBill, string ActiveStatus, string FormNo)
        {
            return objOfferAPI.GetValidOfferList(Doj, UpgradeDate, IsFirstBill, ActiveStatus,  FormNo);
        }        
        public ResponseDetail SaveOffer(Offer ObjOffer)
        {
            return objOfferAPI.saveOfferDynamicNew(ObjOffer);
        }
        public List<Offer> GetAllOffers()
        {
            return objOfferAPI.GetAllOffers();
        }
        public Offer getOfferDetail(int id,string CustId)
        {
            return objOfferAPI.getOfferDetail(id, CustId);
        }
        public List<OfferProduct> GetOfferProductsList(int id)
        {
            return (objOfferAPI.GetOfferProductsList(id));
        }
        public ResponseDetail CheckOfferName(string offerName, int offerID)
        {
            return (objOfferAPI.CheckOfferName(offerName,offerID));
        }
        public string CheckbtgtOffers(string checkwith, string check)
        {
            return (objOfferAPI.CheckbtgtOffers(checkwith, check));
        }
        public List<OfferProduct> getProductsForOffer(string offerID)
        {
            return (objOfferAPI.getProductsForOffer(offerID));
        }

        }
}