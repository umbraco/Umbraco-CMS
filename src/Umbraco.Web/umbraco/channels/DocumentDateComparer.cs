using System;
using System.Collections;
using System.Text;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
namespace umbraco.presentation.channels
{
    class DocumentDateComparer : IComparer
    {

        int IComparer.Compare(Object x, Object y)
        {

            if (((Document)x).CreateDateTime > ((Document)y).CreateDateTime)
            {
                return -1;
            }
            else
            {
                return 0;
            }

        }

    }


    class DocumentSortOrderComparer : IComparer
    {

        int IComparer.Compare(Object x, Object y)
        {

            if (((CMSNode)x).sortOrder > ((CMSNode)y).sortOrder)
            {
                return -1;
            }
            else
            {
                return 0;
            }

        }

    }


}
