namespace TestingAuth.Sample.Api.Get
{
   internal class MergeObjAPI
   {
      public string sourcePatientId;
      public string destinationPatientId;
   }

   internal class ProcessClaimsShield_API
   {
      public string [] shieldServiceIds;
   }

   internal class ProcessClaimsCHLA_API
   {
      public string[] chlaServiceIds;
   }

   internal class ProcessClaimsTests_API
   {
      public string[] testResultIds;

   }

   internal class ProcessClaimsVAX_API
   {
      public string[] vaccinationIds;
   }
}