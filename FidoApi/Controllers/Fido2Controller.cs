using Fido2NetLib.Objects;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FidoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Fido2Controller : ControllerBase
    {        
        private readonly IFido2 _fido2;
        private readonly ILogger<Fido2Controller> _logger;

        public Fido2Controller(ILogger<Fido2Controller> logger, IFido2 fido2)
        {
            _logger = logger;
            _fido2 = fido2;
        }

        [HttpPost]
        [Route("/makeCredentialOptions")]
        public IActionResult MakeCredentialOptions(string username,
                                            string displayName,
                                            string attType,
                                            string authType,
                                            string residentKey= "Preferred",
                                            string userVerification = "Required")
        {
            try
            {
                // 1. Get user from DB by username (in our example, auto create missing users)
                var user = new Fido2User
                {
                    DisplayName = displayName,
                    Name = username,
                    Id = Encoding.UTF8.GetBytes(username) // byte representation of userID is required
                };

                // 2. Get user existing keys by username
                var existingKeys = new List<PublicKeyCredentialDescriptor>();

                // 3. Create options
                var authenticatorSelection = new AuthenticatorSelection
                {
                    RequireResidentKey = residentKey== "Preferred",
                    UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
                };

                if (!string.IsNullOrEmpty(authType))
                    authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();

                var exts = new AuthenticationExtensionsClientInputs()
                {
                    Extensions = true,
                    UserVerificationMethod = true,
                    //DevicePubKey = new AuthenticationExtensionsDevicePublicKeyInputs() { Attestation = attType },
                    //CredProps = true
                };

                var options = _fido2.RequestNewCredential(user, existingKeys, authenticatorSelection, attType.ToEnum<AttestationConveyancePreference>(), exts);

                // 4. Temporarily store options, session/in-memory cache/redis/db
                HttpContext.Session.SetString("fido2.attestationOptions", options.ToJson());

                // 5. return options to client
                _logger.LogInformation("makeCredentialOptions: ", options);
                return Ok(options);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                var optionsError = new CredentialCreateOptions { Status = "error", ErrorMessage = e.Message };
                return BadRequest(optionsError);
            }
        }
    }
}