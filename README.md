# Octopus Terraform Database Importer

## Overview

Importer existing Octopus Stuff into terraform files, including state files.

## Users Beware!

This application is not fully fleshed out and in all honesty, pretty hacky.  I designed this application to allow me to migrate an extensively used Octopus Instance to Terraform.  
The application skips a few things we don't normally use, such as Certificate Variables.  Honestly, I will only add stuff that I intend on using however, if you come across this and feel like you want to contribute, by all means.

Much luck to you!

## About sensitive variables

Octopus Encrypts senstive variables using a salt + database master key.  You need to set the db master key as an environment variable in order to decrypt the sensitve variables.  Otherwise the application just plops the 
encrypted form of the varible in the places.

DO NOT.. for the love of all things holy and green and good in this world commit these directly to any repos.  Take the time to put the secrets in a vault or something.  The state files have to have the sensitive values in the
file for it to do it's thing.  Do not modify them directly.  Just do not commit your state file.  It needs to go into a store of some kind.  Please see terraform's documentation on securing your state file.

## What you need to know to use this

This is not meant to be a user friendly application.  Just a work horse to do a thing.  I have no intention of publishing this for ease of use.  

I've made the following assumptions.
1. That you know how to use Visual Studio or VS Code to run the app.
2. You understand what the state file is in terraform and know how to manipulate it
3. You understand where to find various information in octopus, such as the database master key.
4. That you fully understand that you are going to decrypt a bunch of sensitive values using the database master key
5. That you fully understand that you should never ever ever ever... EVER... commit decrypted values of the tf file OR state file to git repo, even a private one.


### How to use it
1. Clone this repo, open it up in Visual Studio or VS Code. 
2. Create a launchsettings.json file in the Solution properties folder

```json
{
  "profiles": {
    "Octopus.Terraform.VariableSet.Importer": {
      "commandName": "Project",
      "commandLineArgs": "ACTION CMD HERE",
      "environmentVariables": {
        "OCTOPUS_MASTER_KEY" : "DATABASE MASTER KEY HERE",
        "PRIVATE_VARIABLE" : "PRIVATE VARIABLE HERE"
        "OCTOPUS_CONN_STRING" : "SQL CONNECTION STRING HERE"
      }
    }
  }
}
```
Note: I have no idea what the "PRIVATE VARIABLE" is used for in the terraform state file, however I didn't exclude it because it seems like its important.  It seems like its the same value in
all the terraform resources so if you already have a state file just look in there for it.  If you don't, just create a simple resource in octo using terraform and it should show in there.  Again... good luck.

3. Press run and let the magic happen

## Usage

### Import Variable Set
Note: the generated code is meant to work with a custom module I wrote for variable sets

<https://github.com/eternalapprentice2000/octopus-terrafor-libraryset-module> to check that out.

```text
    import-variable-set <variable-set-number-id> <tf-module-name> <out-file>

    ex:
        import-variable-set 2 vs_common_vars c:\dev\test.tf

Output Files:
    "{out-file}"                    : terraform module file  
    "{out-file}.libraryset.json"    : the segement of the state file for the library set  
    "{out-file}.variables.json"     : the segment of the state file for the variables
```

## Modify your Terraform Project

1. Take the Generated TF files and plop them into your project
    * you will need to run `terraform init` in order to get the modules downloaded
2. Copy the state file segments into your state file
3. Run `terraform plan` to make sure all is good.
    * Some items might change a little and if you have a variable type that isn't yet supported in the list, you might get all kinds of weirdness. 
    * check the plan steps to be sure all is good
4. run `terraform apply` to finalize the state and get everything in sync.


