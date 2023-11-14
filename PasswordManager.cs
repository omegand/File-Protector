using System.Text;

namespace FileProtector;

public static class PasswordManager
{
    private const int KeyDerivationSecondSize = 1024;

    /// <summary>
    /// Gets the password from the registry or creates one if it doesn't exist.
    /// </summary>
    /// <returns>The registry password as a byte array.</returns>
    public static byte[] GetPassword()
    {
        object? key = RegistryOperations.GetValue(RegistryOperations.registryAesKey);
        return key == null ? CreatePassword() : (byte[])key;
    }

    /// <summary>
    /// Verifies the provided password against the saved password.
    /// </summary>
    /// <param name="inputPassword">The input password to verify.</param>
    /// <param name="savedPassword">The saved password to compare against.</param>
    /// <returns>The AES key if the password is correct; otherwise, null.</returns>
    public static byte[]? VerifyPassword(string inputPassword, byte[] savedPassword)
    {
        byte[] passwordBytes = ToBytes(inputPassword);
        byte[] aesKey = KeyDerivation.DeriveKey(passwordBytes);
        byte[] secondaryKey = KeyDerivation.DeriveKey(aesKey, KeyDerivationSecondSize); // Using Argon2 again to protect the AES key

        return Utility.BytesEqual(secondaryKey, savedPassword) ? aesKey : null;
    }

    private static byte[] CreatePassword()
    {
        Console.WriteLine("Creating a new password... (If you have already created an input for this PC, use the same one again)");
        string[] passwords = new string[3];
        passwords[0] = Utility.GetInput("Enter your password: ");
        passwords[1] = Utility.GetInput("Repeat your password: ");
        passwords[2] = Utility.GetInput("Repeat your password again: ");

        if (passwords.All(p => p == passwords[0]))
        {
            Program.CurrentPassword = passwords[0];
            return CreateAndSaveKey(passwords[0]);
        }
        else
        {
            Console.WriteLine("\nPasswords do not match. Please try again.");
            return CreatePassword();
        }
    }

    private static byte[] CreateAndSaveKey(string password)
    {
        byte[] passwordBytes = ToBytes(password);
        byte[] aesKey = KeyDerivation.DeriveKey(passwordBytes);
        byte[] secondaryKey = KeyDerivation.DeriveKey(aesKey, KeyDerivationSecondSize); // Using Argon2 again to protect the AES key
        RegistryOperations.SaveIntoRegistry(secondaryKey);
        Console.WriteLine("\nPassword created successfully!");
        Console.WriteLine(password);
        return secondaryKey;
    }
    public static byte[] ToBytes(string str) => Encoding.UTF8.GetBytes(str);
}