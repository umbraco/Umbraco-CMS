using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using umbraco;
using System;
using umbraco.cms.businesslogic;


namespace Umbraco.Web.WebServices
{
    public class DraggableController : UmbracoAuthorizedController
    {
        /// <summary>
        /// Check to see if a content node is allowed to be the child of another node.
        /// </summary>
        /// <param name="app">content tree</param>
        /// <param name="id">node id</param>
        /// <param name="parent">parent id</param>
        /// <returns>xml results true/false</returns>
        [HttpGet]
        public JsonResult CanHaveNode(string app, int id, int parent)
        {
            // If this is a media item.
            if (app == "media")
            {
                // Get the node.
                //var node = new Media(id);
                var node = Services.MediaService.GetById(id);

                // If the parent has changed move the item.
                if (parent != node.ParentId)
                {
                    if (parent == -1)
                    {
                        /* Uncomment for root level content type checking on umbraco 6.0+ */
                        if (!node.ContentType.AllowedAsRoot)
                        {
                            // Content not allowed.
                            return Json(new
                            {
                                success = false
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (parent > 0)
                    {
                        var parentNode = Services.MediaService.GetById(parent);
                        if (!parentNode.ContentType.AllowedContentTypes.Select(s => s.Id.Value).Contains(node.ContentType.Id))
                        {
                            return Json(new
                            {
                                success = false
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }

                }

                // If no disallow rule has been found then alllow the move.
                return Json(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            if (app == "content")
            {
                // Get the document.
                var node = Services.ContentService.GetById(id);

                // If the parent has changed move the item.
                if (parent != node.ParentId)
                {
                    if (parent == -1)
                    {
                        /* Uncomment for root level content type checking on umbraco 6.0+ */
                        if (!node.ContentType.AllowedAsRoot)
                        {
                            // Content not allowed.
                            return Json(new
                            {
                                success = false
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (parent > 0)
                    {
                        var parentNode = Services.ContentService.GetById(parent);
                        if (!parentNode.ContentType.AllowedContentTypes.Select(s => s.Id.Value).Contains(node.ContentType.Id))
                        {
                            // Content not allowed.
                            return Json(new
                            {
                                success = false
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                // If no disallow rule has been found then alllow the move.
                return Json(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }

            // Return a error message.
            return Json(new
            {
                success = false
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Move a Content or Media item to a different location with client side drag and drop.
        /// </summary>
        /// <param name="app">Tree the item belongs to. (media|content)</param>
        /// <param name="id">Node ID of the item to move.</param>
        /// <param name="parent">Node ID of the parent to move the node to.</param>
        /// <param name="index">Sort index to use to order the item.</param>
        [HttpPost]
        public JsonResult MoveNode(string app, int id, int parent, int index)
        {
            // If this is a media item.
            if (app == "media")
            {
                // Get the node.
                var node = Services.MediaService.GetById(id);
                // If the parent has changed move the item.
                if (parent != node.ParentId)
                {
                    if (parent == -1)
                    {
                        // Uncomment for root level content type checking on umbraco 6.0+ */
                        if (!node.ContentType.AllowedAsRoot)
                        {
                            return Json(new
                            {
                                success = false,
                                message = ui.Text("moveNode", "notAllowedAtRoot", node.ContentType.Name, UmbracoUser)
                            });
                        }
                    }
                    else if (parent > 0)
                    {
                        var parentNode = Services.MediaService.GetById(parent);
                        if (!parentNode.ContentType.AllowedContentTypes.Select(s => s.Id.Value).Contains(node.ContentType.Id))
                        {
                            return Json(new
                            {
                                success = false,
                                message = ui.Text("moveNode", "notAllowedInNode", new string[] {node.ContentType.Name, parentNode.ContentType.Name}, UmbracoUser)
                            });
                           
                        }
                    }
                    
                    // Move the node.
                    Services.MediaService.Move(node, parent);
                }

                // Update the sort order.
                node.SortOrder = index;

                // Return a success status.
                return Json(new
                {
                    success = true,
                    message = ui.Text("moveNode", "nodeMoved", new string[] { "media", node.Name, new CMSNode(parent).Text }, UmbracoUser)
                });
            }

            // If this is a content item.
            if (app == "content")
            {
                // Get the document.
                var node = Services.ContentService.GetById(id);

                // If the parent has changed move the item.
                if (parent != node.ParentId)
                {
                    if (parent == -1)
                    {
                        /* Uncomment for root level content type checking on umbraco 6.0+ */
                        if (!node.ContentType.AllowedAsRoot)
                        {
                            return Json(new
                            {
                                success = false,
                                message = ui.Text("moveNode", "notAllowedAtRoot", node.ContentType.Name, UmbracoUser)
                            });
                        }
                    }
                    else if (parent > 0)
                    {
                        var parentNode = Services.ContentService.GetById(parent);
                        if (!parentNode.ContentType.AllowedContentTypes.Select(s => s.Id.Value).Contains(node.ContentType.Id))
                        {
                            return Json(new
                            {
                                success = false,
                                message = ui.Text("moveNode", "notAllowedInNode", new string[] { node.ContentType.Name, parentNode.ContentType.Name }, UmbracoUser)
                            });
                           
                        }
                    }

                    // Move the document.
                    Services.ContentService.Move(node, parent);

                    library.RefreshContent();
                    Services.ContentService.Publish(node);

                    // Publish the document to update it's URL.
                    //node.Publish(umbraco.BusinessLogic.User.GetCurrent());

                    // Refresh the document cache.
                    //library.UpdateDocumentCache(node.Id);
                    // Refresh the sml cache.
                    //System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
                    //node.XmlGenerate(xd);
                    
                    // Still needed?
                    library.RefreshContent();
                }
                // Update the sort order.
                node.SortOrder = index;

                // Return a sucess message.
                return Json(new
                {
                    success = true,
                    message = ui.Text("moveNode", "nodeMoved", new string[] { "document", node.Name, new CMSNode(parent).Text }, UmbracoUser)
                });
            }

            // Return a error message.
            return Json(new
            {
                success = true,
                message = ui.Text("moveNode", "treeNotSupported", app, UmbracoUser)
            });
        }
    }
}
