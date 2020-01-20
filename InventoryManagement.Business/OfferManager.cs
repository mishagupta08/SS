using InventoryManagement.DataAccess;
using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagement.Business
{
    public class OfferManager
    {
        OfferRepository objOfferRepo = new OfferRepository();
        public ResponseDetail SaveOffer(Offer ObjOffer)
        {
            return objOfferRepo.SaveOffer(ObjOffer);
        }

        public List<Offer> GetAllOffers()
        {
            return objOfferRepo.GetAllOffers();
        }

        public List<Offer> GetValidOfferList(string Doj, string UpgradeDate, string IsFirstBill, string ActiveStatus, string FormNo)
        {
            return objOfferRepo.GetValidOfferList(Doj, UpgradeDate, IsFirstBill, ActiveStatus,  FormNo);
        }

        public Offer getOfferDetail(int id,string CustId)
        {
            return objOfferRepo.getOfferDetail(id, CustId);
        }

        public List<OfferProduct> GetOfferProductsList(int id)
        {
            return (objOfferRepo.GetOfferProductsList(id));
        }
        public ResponseDetail CheckOfferName(string offerName, int offerID)
        {
            return (objOfferRepo.CheckOfferName(offerName,offerID));
        }
        public string CheckbtgtOffers(string checkwith, string check)
        {
            return (objOfferRepo.CheckbtgtOffers(checkwith, check));
        }
        public List<OfferProduct> getProductsForOffer(string offerID)
        {
            return (objOfferRepo.getProductsForOffer(offerID));
        }
    }
}