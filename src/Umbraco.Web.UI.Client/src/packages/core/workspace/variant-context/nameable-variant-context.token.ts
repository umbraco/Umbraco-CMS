import { type UmbVariantContext } from "./variant-context.interface.js";
import { UmbNameableVariantContext } from "./nameable-variant-context.interface.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const IsNameablePropertySetContext = (context: UmbVariantContext): context is UmbNameableVariantContext => 'setName' in context;

export const UMB_NAMEABLE_VARIANT_CONTEXT = new UmbContextToken<UmbVariantContext, UmbNameableVariantContext>(
	"UmbVariantContext",
	IsNameablePropertySetContext);
