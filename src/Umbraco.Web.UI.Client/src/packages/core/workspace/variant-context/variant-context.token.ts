import { type UmbVariantContext } from "./variant-context.interface.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const UMB_VARIANT_CONTEXT = new UmbContextToken<UmbVariantContext>("UmbVariantContext");
