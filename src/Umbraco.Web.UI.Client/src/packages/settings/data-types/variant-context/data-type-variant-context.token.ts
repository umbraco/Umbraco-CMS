import type { UmbDataTypeVariantContext } from "./data-type-variant-context.js";
import { UmbVariantContext } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const isDataTypeVariantContext = (context: UmbVariantContext): context is UmbDataTypeVariantContext => ('properties' in context && context.getType() === 'data-type');

export const UMB_DATA_TYPE_VARIANT_CONTEXT = new UmbContextToken<UmbVariantContext, UmbDataTypeVariantContext>(
	"UmbVariantContext", isDataTypeVariantContext);
