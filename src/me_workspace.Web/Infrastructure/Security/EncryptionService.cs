namespace me_workspace.Web.Infrastructure.Security;

public sealed class EncryptionService
{
    public string Protect(string plainText) => plainText;

    public string Unprotect(string cipherText) => cipherText;
}
