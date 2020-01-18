using InventoryManagement.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InventoryManagement.DataAccess;

namespace InventoryManagement.Business
{
    public class OfferMaster
    {
        OfferRepository objTransacRepo = new OfferRepository();

        public Offer GetSelectedOfferDetails(decimal OfferId)
        {
            return (objTransacRepo.GetSelectedOfferDetails(OfferId));
        }
        public List<OfferProducts> GetSelectedOfferProductList(decimal OfferId)
        {
            return (objTransacRepo.GetSelectedOfferProductList(OfferId));
        }
        public ResponseDetail SaveOffer(Offer offerDetail)
        {
            ResponseDetail objResponse = objTransacRepo.SaveOffer(offerDetail);
            return objResponse;
        }
        public List<Offer> GetAllOfferList(string OfferType)
        {
            return (objTransacRepo.GetAllOfferList(OfferType));
        }

    }
}