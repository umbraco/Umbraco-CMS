import type { UmbDocumentVariantContext } from "./document-variant-context.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbVariantContext } from "@umbraco-cms/backoffice/workspace";

export const IsDocumentVariantContext = (context: UmbVariantContext): context is UmbDocumentVariantContext => context.getType() === 'document';

export const UMB_DOCUMENT_VARIANT_CONTEXT = new UmbContextToken<UmbVariantContext, UmbDocumentVariantContext>(
	"UmbVariantContext",
	IsDocumentVariantContext);
