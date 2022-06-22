## Umbraco Test Data

This project is a utility to be able to generate large amounts of content and media in an
Umbraco installation for testing.

## Usage

You must use SQL Server for this, using SQLCE will die if you try to bulk create huge amounts of data.

It has to be enabled by an appSetting:

```json
{
  "Umbraco": {
    "CMS": {
      "TestData": {
        "Enabled" : true,
      }
    }
  }
}
```

Once this is enabled this endpoint can be executed:

`/umbraco/surface/umbracotestdata/CreateTree?count=100&depth=5`

The query string options are:

* `count` = the number of content and media nodes to create
* `depth` = how deep the trees created will be
* `locale` (optional, default = "en") = the language that the data will be generated in

This creates a content and associated media tree (hierarchy). Each content item created is associated
to a media item via a media picker and therefore a relation is created between the two. Each content and
media tree created have the same root node name so it's easy to know which content branch relates to
which media branch.

All values are generated using the very handy `Bogus` package. 

## Schema

This will install some schema items:

* `umbTestDataContent` Document Type. __TIP__: If you want to delete all of the content data generated with this tool, just delete this content type
* `UmbracoTestDataContent.RTE` Data Type
* `UmbracoTestDataContent.MediaPicker` Data Type
* `UmbracoTestDataContent.Text` Data Type

For media, the normal folder and image is used

## Media

This does not upload physical files, it just uses a randomized online image as the `umbracoFile` value.
This works when viewing the media item in the media section and the image will show up and with recent changes this will also work
when editing content to view the thumbnail for the picked media. 
