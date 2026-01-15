import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbIncomingHintBase {
	unique?: string | symbol;
	text: string;
	weight?: number;
	color?: UUIInterfaceColor;
}

export interface UmbHint extends UmbIncomingHintBase {
	unique: string | symbol;
	path: Array<string>;
	weight: number;
}

export interface UmbVariantHint extends UmbHint {
	variantId?: UmbVariantId;
}
