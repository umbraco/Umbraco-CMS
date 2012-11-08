Umbraco Base Rest Handler
-------------------------

The Umbraco Base Rest Handler has moved to Umbraco.Web.BaseRest.

At the moment, both the legacy way and the new way of doing things are supported.
The baseHttpModule does _not_ run anymore, everything is handled by
Umbraco.Web.BaseRest, but the legacy attributes and config files are supported,
so legacy extensions are discovered and will run properly.

However, you should start using the attributes in Umbraco.Web.BaseRest, and the
new BaseRestExtensions.config config file.

The legacy system will be obsoleted at some point in the future and all references
to the code legacy will be removed.

--