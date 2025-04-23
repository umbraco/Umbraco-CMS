using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Controllers;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi;

[TestFixture]
internal sealed class OpenApiContractTest : UmbracoTestServerTestBase
{
    private IHostingEnvironment HostingEnvironment => GetRequiredService<IHostingEnvironment>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddMvcAndRazor(mvcBuilder =>
        {
            // Adds Umbraco.Cms.Api.Delivery
            mvcBuilder.AddApplicationPart(typeof(DeliveryApiControllerBase).Assembly);
        });

    [Test]
    public async Task Validate_OpenApi_Contract()
    {
        var backOfficePath = HostingEnvironment.GetBackOfficePath();

        var swaggerPath = $"{backOfficePath}/swagger/delivery/swagger.json";

        var generatedOpenApiContract = await Client.GetStringAsync(swaggerPath);
        var generatedOpenApiJson = JsonNode.Parse(generatedOpenApiContract);
        var expectedOpenApiJson = JsonNode.Parse(ExpectedOpenApiContract);

        Assert.NotNull(generatedOpenApiJson);
        Assert.NotNull(expectedOpenApiJson);

        Assert.AreEqual(expectedOpenApiJson.ToJsonString(), generatedOpenApiJson.ToJsonString(), $"Generated API do not respect the contract.");
    }

    private const string ExpectedOpenApiContract =
    """
    {
      "openapi": "3.0.1",
      "info": {
        "title": "Umbraco Delivery API",
        "description": "You can find out more about the Umbraco Delivery API in [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api).",
        "version": "Latest"
      },
      "paths": {
        "/umbraco/delivery/api/v2/content": {
          "get": {
            "tags": [
              "Content"
            ],
            "operationId": "GetContent2.0",
            "parameters": [
              {
                "name": "fetch",
                "in": "query",
                "description": "Specifies the content items to fetch. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Select all": {
                    "value": ""
                  },
                  "Select all ancestors of a node by id": {
                    "value": "ancestors:id"
                  },
                  "Select all ancestors of a node by path": {
                    "value": "ancestors:path"
                  },
                  "Select all children of a node by id": {
                    "value": "children:id"
                  },
                  "Select all children of a node by path": {
                    "value": "children:path"
                  },
                  "Select all descendants of a node by id": {
                    "value": "descendants:id"
                  },
                  "Select all descendants of a node by path": {
                    "value": "descendants:path"
                  }
                }
              },
              {
                "name": "filter",
                "in": "query",
                "description": "Defines how to filter the fetched content items. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "array",
                  "items": {
                    "type": "string"
                  }
                },
                "examples": {
                  "Default filter": {
                    "value": ""
                  },
                  "Filter by content type (equals)": {
                    "value": [
                      "contentType:alias1"
                    ]
                  },
                  "Filter by name (contains)": {
                    "value": [
                      "name:nodeName"
                    ]
                  },
                  "Filter by creation date (less than)": {
                    "value": [
                      "createDate<2024-01-01"
                    ]
                  },
                  "Filter by update date (greater than or equal)": {
                    "value": [
                      "updateDate>:2023-01-01"
                    ]
                  }
                }
              },
              {
                "name": "sort",
                "in": "query",
                "description": "Defines how to sort the found content items. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "array",
                  "items": {
                    "type": "string"
                  }
                },
                "examples": {
                  "Default sort": {
                    "value": ""
                  },
                  "Sort by create date": {
                    "value": [
                      "createDate:asc",
                      "createDate:desc"
                    ]
                  },
                  "Sort by level": {
                    "value": [
                      "level:asc",
                      "level:desc"
                    ]
                  },
                  "Sort by name": {
                    "value": [
                      "name:asc",
                      "name:desc"
                    ]
                  },
                  "Sort by sort order": {
                    "value": [
                      "sortOrder:asc",
                      "sortOrder:desc"
                    ]
                  },
                  "Sort by update date": {
                    "value": [
                      "updateDate:asc",
                      "updateDate:desc"
                    ]
                  }
                }
              },
              {
                "name": "skip",
                "in": "query",
                "description": "Specifies the number of found content items to skip. Use this to control pagination of the response.",
                "schema": {
                  "type": "integer",
                  "format": "int32",
                  "default": 0
                }
              },
              {
                "name": "take",
                "in": "query",
                "description": "Specifies the number of found content items to take. Use this to control pagination of the response.",
                "schema": {
                  "type": "integer",
                  "format": "int32",
                  "default": 10
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Accept-Language",
                "in": "header",
                "description": "Defines the language to return. Use this when querying language variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "English culture": {
                    "value": "en-us"
                  }
                }
              },
              {
                "name": "Accept-Segment",
                "in": "header",
                "description": "Defines the segment to return. Use this when querying segment variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "Segment One": {
                    "value": "segment-one"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              },
              {
                "name": "Preview",
                "in": "header",
                "description": "Whether to request draft content.",
                "schema": {
                  "type": "boolean"
                }
              },
              {
                "name": "Start-Item",
                "in": "header",
                "description": "URL segment or GUID of a root content item.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/PagedIApiContentResponseModel"
                        }
                      ]
                    }
                  }
                }
              },
              "400": {
                "description": "Bad Request",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/ProblemDetails"
                        }
                      ]
                    }
                  }
                }
              },
              "404": {
                "description": "Not Found"
              }
            }
          }
        },
        "/umbraco/delivery/api/v2/content/item/{path}": {
          "get": {
            "tags": [
              "Content"
            ],
            "operationId": "GetContentItemByPath2.0",
            "parameters": [
              {
                "name": "path",
                "in": "path",
                "required": true,
                "schema": {
                  "type": "string",
                  "default": ""
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Accept-Language",
                "in": "header",
                "description": "Defines the language to return. Use this when querying language variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "English culture": {
                    "value": "en-us"
                  }
                }
              },
              {
                "name": "Accept-Segment",
                "in": "header",
                "description": "Defines the segment to return. Use this when querying segment variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "Segment One": {
                    "value": "segment-one"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              },
              {
                "name": "Preview",
                "in": "header",
                "description": "Whether to request draft content.",
                "schema": {
                  "type": "boolean"
                }
              },
              {
                "name": "Start-Item",
                "in": "header",
                "description": "URL segment or GUID of a root content item.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/ApiContentResponseModel"
                        }
                      ]
                    }
                  }
                }
              },
              "401": {
                "description": "Unauthorized"
              },
              "403": {
                "description": "Forbidden"
              },
              "404": {
                "description": "Not Found"
              }
            }
          }
        },
        "/umbraco/delivery/api/v2/content/item/{id}": {
          "get": {
            "tags": [
              "Content"
            ],
            "operationId": "GetContentItemById2.0",
            "parameters": [
              {
                "name": "id",
                "in": "path",
                "required": true,
                "schema": {
                  "type": "string",
                  "format": "uuid"
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Accept-Language",
                "in": "header",
                "description": "Defines the language to return. Use this when querying language variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "English culture": {
                    "value": "en-us"
                  }
                }
              },
              {
                "name": "Accept-Segment",
                "in": "header",
                "description": "Defines the segment to return. Use this when querying segment variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "Segment One": {
                    "value": "segment-one"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              },
              {
                "name": "Preview",
                "in": "header",
                "description": "Whether to request draft content.",
                "schema": {
                  "type": "boolean"
                }
              },
              {
                "name": "Start-Item",
                "in": "header",
                "description": "URL segment or GUID of a root content item.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/ApiContentResponseModel"
                        }
                      ]
                    }
                  }
                }
              },
              "401": {
                "description": "Unauthorized"
              },
              "403": {
                "description": "Forbidden"
              },
              "404": {
                "description": "Not Found"
              }
            }
          }
        },
        "/umbraco/delivery/api/v2/content/items": {
          "get": {
            "tags": [
              "Content"
            ],
            "operationId": "GetContentItems2.0",
            "parameters": [
              {
                "name": "id",
                "in": "query",
                "schema": {
                  "uniqueItems": true,
                  "type": "array",
                  "items": {
                    "type": "string",
                    "format": "uuid"
                  }
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Accept-Language",
                "in": "header",
                "description": "Defines the language to return. Use this when querying language variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "English culture": {
                    "value": "en-us"
                  }
                }
              },
              {
                "name": "Accept-Segment",
                "in": "header",
                "description": "Defines the segment to return. Use this when querying segment variant content items.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Default": {
                    "value": ""
                  },
                  "Segment One": {
                    "value": "segment-one"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              },
              {
                "name": "Preview",
                "in": "header",
                "description": "Whether to request draft content.",
                "schema": {
                  "type": "boolean"
                }
              },
              {
                "name": "Start-Item",
                "in": "header",
                "description": "URL segment or GUID of a root content item.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "type": "array",
                      "items": {
                        "oneOf": [
                          {
                            "$ref": "#/components/schemas/ApiContentResponseModel"
                          }
                        ]
                      }
                    }
                  }
                }
              },
              "401": {
                "description": "Unauthorized"
              },
              "403": {
                "description": "Forbidden"
              }
            }
          }
        },
        "/umbraco/delivery/api/v2/media": {
          "get": {
            "tags": [
              "Media"
            ],
            "operationId": "GetMedia2.0",
            "parameters": [
              {
                "name": "fetch",
                "in": "query",
                "description": "Specifies the media items to fetch. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Select all children at root level": {
                    "value": "children:/"
                  },
                  "Select all children of a media item by id": {
                    "value": "children:id"
                  },
                  "Select all children of a media item by path": {
                    "value": "children:path"
                  }
                }
              },
              {
                "name": "filter",
                "in": "query",
                "description": "Defines how to filter the fetched media items. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "array",
                  "items": {
                    "type": "string"
                  }
                },
                "examples": {
                  "Default filter": {
                    "value": ""
                  },
                  "Filter by media type": {
                    "value": [
                      "mediaType:alias1"
                    ]
                  },
                  "Filter by name": {
                    "value": [
                      "name:nodeName"
                    ]
                  }
                }
              },
              {
                "name": "sort",
                "in": "query",
                "description": "Defines how to sort the found media items. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "array",
                  "items": {
                    "type": "string"
                  }
                },
                "examples": {
                  "Default sort": {
                    "value": ""
                  },
                  "Sort by create date": {
                    "value": [
                      "createDate:asc",
                      "createDate:desc"
                    ]
                  },
                  "Sort by name": {
                    "value": [
                      "name:asc",
                      "name:desc"
                    ]
                  },
                  "Sort by sort order": {
                    "value": [
                      "sortOrder:asc",
                      "sortOrder:desc"
                    ]
                  },
                  "Sort by update date": {
                    "value": [
                      "updateDate:asc",
                      "updateDate:desc"
                    ]
                  }
                }
              },
              {
                "name": "skip",
                "in": "query",
                "description": "Specifies the number of found media items to skip. Use this to control pagination of the response.",
                "schema": {
                  "type": "integer",
                  "format": "int32",
                  "default": 0
                }
              },
              {
                "name": "take",
                "in": "query",
                "description": "Specifies the number of found media items to take. Use this to control pagination of the response.",
                "schema": {
                  "type": "integer",
                  "format": "int32",
                  "default": 10
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/PagedIApiMediaWithCropsResponseModel"
                        }
                      ]
                    }
                  }
                }
              },
              "400": {
                "description": "Bad Request",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/ProblemDetails"
                        }
                      ]
                    }
                  }
                }
              }
            }
          }
        },
        "/umbraco/delivery/api/v2/media/item/{path}": {
          "get": {
            "tags": [
              "Media"
            ],
            "operationId": "GetMediaItemByPath2.0",
            "parameters": [
              {
                "name": "path",
                "in": "path",
                "required": true,
                "schema": {
                  "type": "string"
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/ApiMediaWithCropsResponseModel"
                        }
                      ]
                    }
                  }
                }
              },
              "404": {
                "description": "Not Found"
              }
            }
          }
        },
        "/umbraco/delivery/api/v2/media/item/{id}": {
          "get": {
            "tags": [
              "Media"
            ],
            "operationId": "GetMediaItemById2.0",
            "parameters": [
              {
                "name": "id",
                "in": "path",
                "required": true,
                "schema": {
                  "type": "string",
                  "format": "uuid"
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "oneOf": [
                        {
                          "$ref": "#/components/schemas/ApiMediaWithCropsResponseModel"
                        }
                      ]
                    }
                  }
                }
              },
              "404": {
                "description": "Not Found"
              }
            }
          }
        },
        "/umbraco/delivery/api/v2/media/items": {
          "get": {
            "tags": [
              "Media"
            ],
            "operationId": "GetMediaItems2.0",
            "parameters": [
              {
                "name": "id",
                "in": "query",
                "schema": {
                  "uniqueItems": true,
                  "type": "array",
                  "items": {
                    "type": "string",
                    "format": "uuid"
                  }
                }
              },
              {
                "name": "expand",
                "in": "query",
                "description": "Defines the properties that should be expanded in the response. Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Expand none": {
                    "value": ""
                  },
                  "Expand all properties": {
                    "value": "properties[$all]"
                  },
                  "Expand specific property": {
                    "value": "properties[alias1]"
                  },
                  "Expand specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Expand nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "fields",
                "in": "query",
                "description": "Explicitly defines which properties should be included in the response (by default all properties are included). Refer to [the documentation](https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api#query-parameters) for more details on this.",
                "schema": {
                  "type": "string"
                },
                "examples": {
                  "Include all properties": {
                    "value": "properties[$all]"
                  },
                  "Include only specific property": {
                    "value": "properties[alias1]"
                  },
                  "Include only specific properties": {
                    "value": "properties[alias1,alias2]"
                  },
                  "Include only specific nested properties": {
                    "value": "properties[alias1[properties[nestedAlias1,nestedAlias2]]]"
                  }
                }
              },
              {
                "name": "Api-Key",
                "in": "header",
                "description": "API key specified through configuration to authorize access to the API.",
                "schema": {
                  "type": "string"
                }
              }
            ],
            "responses": {
              "200": {
                "description": "OK",
                "content": {
                  "application/json": {
                    "schema": {
                      "type": "array",
                      "items": {
                        "oneOf": [
                          {
                            "$ref": "#/components/schemas/ApiMediaWithCropsResponseModel"
                          }
                        ]
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "components": {
        "schemas": {
          "ApiContentResponseModel": {
            "required": [
              "contentType",
              "createDate",
              "cultures",
              "id",
              "name",
              "properties",
              "route",
              "updateDate"
            ],
            "type": "object",
            "properties": {
              "id": {
                "type": "string",
                "format": "uuid"
              },
              "contentType": {
                "type": "string"
              },
              "properties": {
                "type": "object",
                "additionalProperties": {
                  "nullable": true
                }
              },
              "name": {
                "type": "string"
              },
              "createDate": {
                "type": "string",
                "format": "date-time"
              },
              "updateDate": {
                "type": "string",
                "format": "date-time"
              },
              "route": {
                "oneOf": [
                  {
                    "$ref": "#/components/schemas/ApiContentRouteModel"
                  }
                ]
              },
              "cultures": {
                "type": "object",
                "additionalProperties": {
                  "oneOf": [
                    {
                      "$ref": "#/components/schemas/ApiContentRouteModel"
                    }
                  ]
                }
              }
            },
            "additionalProperties": false
          },
          "ApiContentRouteModel": {
            "required": [
              "path",
              "startItem"
            ],
            "type": "object",
            "properties": {
              "path": {
                "type": "string"
              },
              "startItem": {
                "oneOf": [
                  {
                    "$ref": "#/components/schemas/ApiContentStartItemModel"
                  }
                ]
              }
            },
            "additionalProperties": false
          },
          "ApiContentStartItemModel": {
            "required": [
              "id",
              "path"
            ],
            "type": "object",
            "properties": {
              "id": {
                "type": "string",
                "format": "uuid"
              },
              "path": {
                "type": "string"
              }
            },
            "additionalProperties": false
          },
          "ApiMediaWithCropsResponseModel": {
            "required": [
              "createDate",
              "id",
              "mediaType",
              "name",
              "path",
              "properties",
              "updateDate",
              "url"
            ],
            "type": "object",
            "properties": {
              "id": {
                "type": "string",
                "format": "uuid",
                "readOnly": true
              },
              "name": {
                "type": "string",
                "readOnly": true
              },
              "mediaType": {
                "type": "string",
                "readOnly": true
              },
              "url": {
                "type": "string",
                "readOnly": true
              },
              "extension": {
                "type": "string",
                "nullable": true,
                "readOnly": true
              },
              "width": {
                "type": "integer",
                "format": "int32",
                "nullable": true,
                "readOnly": true
              },
              "height": {
                "type": "integer",
                "format": "int32",
                "nullable": true,
                "readOnly": true
              },
              "bytes": {
                "type": "integer",
                "format": "int32",
                "nullable": true,
                "readOnly": true
              },
              "properties": {
                "type": "object",
                "additionalProperties": {
                  "nullable": true
                },
                "readOnly": true
              },
              "focalPoint": {
                "oneOf": [
                  {
                    "$ref": "#/components/schemas/ImageFocalPointModel"
                  }
                ],
                "nullable": true
              },
              "crops": {
                "type": "array",
                "items": {
                  "oneOf": [
                    {
                      "$ref": "#/components/schemas/ImageCropModel"
                    }
                  ]
                },
                "nullable": true
              },
              "path": {
                "type": "string"
              },
              "createDate": {
                "type": "string",
                "format": "date-time"
              },
              "updateDate": {
                "type": "string",
                "format": "date-time"
              }
            },
            "additionalProperties": false
          },
          "ImageCropCoordinatesModel": {
            "required": [
              "x1",
              "x2",
              "y1",
              "y2"
            ],
            "type": "object",
            "properties": {
              "x1": {
                "type": "number",
                "format": "double"
              },
              "y1": {
                "type": "number",
                "format": "double"
              },
              "x2": {
                "type": "number",
                "format": "double"
              },
              "y2": {
                "type": "number",
                "format": "double"
              }
            },
            "additionalProperties": false
          },
          "ImageCropModel": {
            "required": [
              "height",
              "width"
            ],
            "type": "object",
            "properties": {
              "alias": {
                "type": "string",
                "nullable": true
              },
              "width": {
                "type": "integer",
                "format": "int32"
              },
              "height": {
                "type": "integer",
                "format": "int32"
              },
              "coordinates": {
                "oneOf": [
                  {
                    "$ref": "#/components/schemas/ImageCropCoordinatesModel"
                  }
                ],
                "nullable": true
              }
            },
            "additionalProperties": false
          },
          "ImageFocalPointModel": {
            "required": [
              "left",
              "top"
            ],
            "type": "object",
            "properties": {
              "left": {
                "type": "number",
                "format": "double"
              },
              "top": {
                "type": "number",
                "format": "double"
              }
            },
            "additionalProperties": false
          },
          "PagedIApiContentResponseModel": {
            "required": [
              "items",
              "total"
            ],
            "type": "object",
            "properties": {
              "total": {
                "type": "integer",
                "format": "int64"
              },
              "items": {
                "type": "array",
                "items": {
                  "oneOf": [
                    {
                      "$ref": "#/components/schemas/ApiContentResponseModel"
                    }
                  ]
                }
              }
            },
            "additionalProperties": false
          },
          "PagedIApiMediaWithCropsResponseModel": {
            "required": [
              "items",
              "total"
            ],
            "type": "object",
            "properties": {
              "total": {
                "type": "integer",
                "format": "int64"
              },
              "items": {
                "type": "array",
                "items": {
                  "oneOf": [
                    {
                      "$ref": "#/components/schemas/ApiMediaWithCropsResponseModel"
                    }
                  ]
                }
              }
            },
            "additionalProperties": false
          },
          "ProblemDetails": {
            "type": "object",
            "properties": {
              "type": {
                "type": "string",
                "nullable": true
              },
              "title": {
                "type": "string",
                "nullable": true
              },
              "status": {
                "type": "integer",
                "format": "int32",
                "nullable": true
              },
              "detail": {
                "type": "string",
                "nullable": true
              },
              "instance": {
                "type": "string",
                "nullable": true
              }
            },
            "additionalProperties": { }
          }
        }
      }
    }
    """;
}
