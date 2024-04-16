# KeePassProtectedKeyStore: Creates and Uses Protected Key Stores for KeePass Authentication
[![Latest release](https://img.shields.io/github/release/CSquared167/KeePassProtectedKeyStore.svg?label=latest%20release)](https://github.com/CSquared167/KeePassProtectedKeyStore/releases/latest)
[![GitHub issues](https://img.shields.io/github/issues/CSquared167/KeePassProtectedKeyStore.svg)](https://github.com/CSquared167/KeePassProtectedKeyStore/issues)
[![Github All Releases](https://img.shields.io/github/downloads/CSquared167/KeePassProtectedKeyStore/total.svg)](https://github.com/CSquared167/KeePassProtectedKeyStore/releases)
[![License](https://img.shields.io/github/license/CSquared167/KeePassProtectedKeyStore.svg)](https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/LICENSE)

This plugin for [KeePass 2][KeePass] password manager uses the computer's [Trusted Platform Module] (TPM) hardware to create protected key stores. The encrypted key store files are stored in the `%LOCALAPPDATA%\CSquared167\KeePassProtectedKeyStore` folder.

KeePassProtectedKeyStore includes the following functionality:
- [Convert](#creating-a-protected-key-store) one or more existing authentication keys (master password, Windows user account, and/or a key/file provider) to a protected key store.
- [Create](#creating-an-emergency-key-recovery-file) an emergency key recovery file, in case the protected key store is [no longer available](#when-a-protected-key-store-is-no-longer-available).
- [Import](#importing-an-emergency-key-recovery-file) an emergency key recovery file to recreate a protected key store that is [no longer available](#when-a-protected-key-store-is-no-longer-available).
- [Auto-login](#auto-login-options) to a database for which a protected key store is the only authentication key.
- [Create or reuse](#creating-a-new-master-key-including-the-keepassprotectedkeystore-key-provider) a protected key store when you either create a new database or change the master password of an existing database, and the new master key includes the KeePassProtectedKeyStore key provider.

For users with multiple databases, KeePassProtectedKeyStore can maintain separate protected key stores for each database. For users who have either a single database or multiple databases with the same master key, this plugin can maintain a single default protected key store.

If a shared database is being used (e.g., the database resides on a NAS filesystem and is accessed by multiple computers), KeePassProtectedKeyStore can be used on each computer to generate a protected key store file specific to that Windows user and computer. If an emergency key recovery file is created, it does not need to be created more than once, and it can be imported on any other computer that requires it.

[KeePass]: https://keepass.info/
[Trusted Platform Module]: https://en.wikipedia.org/wiki/Trusted_Platform_Module

## When a Protected Key Store is No Longer Available:
One explanation of when a protected key store is no longer available is when the encrypted key store files become corrupted or are deleted.

Another explanation revolves around how the TPM hardware associates the encrypted key with the Windows account's security identifier (SID) and possibly other attributes. The following are just a few examples of when the SID might be different:
- You purchase a new computer.
- Your computer crashes and you need to rebuild it.
- You do a clean reinstallation of Windows.
- You copy an encrypted protected key store file to another computer.

In these examples, the new Windows account will have a completely different SID. Because the TPM will not allow these types of protected key stores to be decrypted, they are considered to be no longer available.

The KeePass documentation includes additional information related to the [Windows User Account] authentication, which also uses the TPM to encrypt and store a random binary key. Unlike KeePass, KeePassProtectedKeyStore allows you to create an emergency key recovery file to safeguard against these types of situations. If the protected key store is no longer available, the emergency key recovery file can be imported to recerate the protected key store.

[Windows User Account]: https://keepass.info/help/base/keys.html#winuser

## Disclaimer:
KeePassProtectedKeyStore cannot help you if you lose access to your database because you forgot your original master key, didn't create or lost your emergency key recovery file, secured the emergency key recovery file with a password but don't remember what it was, etc.

## How to Install:
- Download [KeePassProtectedKeyStore.zip][binLink] to your computer and extract KeePassProtectedKeyStore.dll from it.
- Open KeePass and select `Tools/Plugins...` from the main menu.
- Click the `Open Folder` button. This will open a Windows Explorer window, set to the location of KeePass' Plugins folder.
- Move or copy KeePassProtectedKeyStore.dll to the Plugins folder. You will probably get a message stating you will need to provide administrator permission. Click on whichever button allows the move/copy to complete.

[binLink]: https://github.com/CSquared167/KeePassProtectedKeyStore/releases "Plugin Releases"

## Options:
The KeePassProtectedKeyStore options are available from KeePass' `Tools` menu:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/KeePassToolsMenu.png?raw=true" />

The KeePassProtectedKeyStore Options dialog will initially look like the following:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/OptionsDialogPreConversion.png?raw=true" />

## Creating a Protected Key Store:
To create a protected key store, click `Convert Existing Authentication Key(s) to a Protected Key Store...` from the KeePassProtectedKeyStore Options dialog. You will first be prompted to specify the type of protected key store you wish to create:
- Default: You can use this option if you have only one database, or if you have multiple databases that use the same master key.
- Individual: You can use this option if you have multiple databases using different master keys.

The screenshot below gives additional details around each type.

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/ProtectedKeyStoreType.png?raw=true" />

You will next be prompted to select the authentication key(s) to convert, even if the database has only one authentication key. The following shows what the prompt will look like for a database that has only a master password:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/ConvertToProtectedKeyStoreDialog1.png?raw=true" />

The following shows what the prompt will look like for a database that has both a master password and a key file. Note that the key file is automatically selected. The reason is that KeePassProtectedKeyStore is a key provider, and KeePass supports only one key file/provider per database. It is therefore impossible to use KeePassProtectedKeyStore in addition to another key/file provider.

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/ConvertToProtectedKeyStoreDialog2.png?raw=true" />

If you attempt to deselect the key file/provider, the entry will be reselected, and the following will be displayed. The warning message will not appear the second and subsequent times the entry is deselected, but the entry will still be reselected.

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/ConvertToProtectedKeyStoreWarningDialog.png?raw=true" />

Once you have created the protected key store, the KeePassProtectedKeyStore Options dialog will look similar to the following, depending on which type of protected key store you chose to create:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/OptionsDialogPostConversion1.png?raw=true" />

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/OptionsDialogPostConversion2.png?raw=true" />

Once you have logged back into the database using the protected key store, the KeePassProtectedKeyStore Options dialog will look similar to the following, depending on which type of protected key store you chose to create:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/OptionsDialogAfterLogin1.png?raw=true" />

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/OptionsDialogAfterLogin2.png?raw=true" />

## Creating an Emergency Key Recovery File:
After creating a protected key store, you will be asked whether you want to create an emergency key recovery file. You can also click `Create an Emergency Key Recovery File...` at any time from the KeePassProtectedKeyStore Options dialog.

The protected key store in the emergency key recovery file is encrypted. As such, you will be asked to select whether you would like KeePassProtectedKeyStore to protect the data or if you would like to protect it yourself by entering a password. As you can see, the advantages and disadvantages of selecting either option are listed.

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/CreateEmergencyKeyRecoveryFileSelectProtectionMethod.png?raw=true" />

If you choose to enter a password yourself, you will be presented with the following:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/CreateEmergencyKeyRecoveryFileCreatePassword.png?raw=true" />

The behavior is similar to that of KeePass when creating a new database or changing the master key of an existing database. The OK button will not be enabled unless the entered and re-entered passwords match, or if the button with the three dots is clicked to show the password.

Once the protection method has been finalized, you will be prompted to save the file. The folder and/or filename can be changed to suit your needs.

**NOTE:** The emergency key recovery file contains ONLY the protected key store created by KeePassProtectedKeyStore. It DOES NOT include the master password or Windows user account keys.

## Importing an Emergency Key Recovery File:
To import an emergency key recovery file, click `Import an Emergency Key Recovery File...` from the KeePassProtectedKeyStore Options dialog. You will first be prompted to select the file to import.

If the file is associated with an individual protected key store, you will also be prompted to confirm the associated database's directory/filename. It is critical for an individual protected key store to be associated with the correct database, or else you will not be able to open the database after importing the key. By default you will be prompted with the directory where the database was located when the emergency key recovery file was created, but you have the option to change it if you moved and/or renamed the database since creating the file.

If you had entered a password to protect the data when the file was created, you will be prompted to confirm the password as follows:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/ImportEmergencyKeyRecoveryFileEnterPassword1.png?raw=true" />

You will be given constant feedback whether you have entered the correct password. The OK button will not be enabled until the entered password is correct.

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/ImportEmergencyKeyRecoveryFileEnterPassword2.png?raw=true" />

## Auto-Login Options:
KeePass displays the "Open Database" dialog when first launching KeePass, when reopening the database after it has been locked, when using the `File/Open` method to open a database, etc. If a protected key store is the only authentication key for a database, it is eligible for auto-login. When auto-login is enabled for the database, the "Open Database" dialog is not shown, and you are logged in directly to the database.

When a protected key store is created, either by initially creating the key or importing an emergency key recovery file, auto-login is enabled by default for that database. The KeePassProtectedKeyStore Options dialog includes a checkbox to change this default behavior. You can also turn on/off auto-login for each database individually.

If you choose to disable auto-login for a particular database, you will need to select the "KeePassProtectedKeyStore" key file/provider option in the "Open Database" dialog, such as the following:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/EnterMasterKeyDialog.png?raw=true" />

The original authentication key(s) that had been converted to the protected key store must no longer be specified in the "Open Database" dialog. If the original authentication keys are used in addition to the protected key store, KeePass will consider them to be separate keys and will attempt to authenticate the database against both keys, which will fail.

The auto-login functionality was inspired by Jeremy Bourgin's [KeePassAutoUnlock] plugin.

[KeePassAutoUnlock]: https://github.com/jeremy-bourgin/KeePassAutoUnlock

## Creating a New Master Key Including the KeePassProtectedKeyStore Key Provider:
If you create a new database or change the master key for an existing database, and you include the KeePassProtectedKeyStore key provider as part of the master key, KeePassProtectedKeyStore will prompt for the type of protected key store you wish to create:

<img src="https://github.com/CSquared167/KeePassProtectedKeyStore/blob/master/Screenshots/ProtectedKeyStoreType.png?raw=true" />

Whichever type you choose, if a protected key store of that type already exists, the existing protected key store will be used as the key. If it does not already exist, a new random key will be auto-generated. In these cases it will be more critical for you to create an emergency key recovery file. Because the key will be unknown to you, you will lose access to the database if the protected key store is [no longer available](#when-a-protected-key-store-is-no-longer-available).
