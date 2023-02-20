import { VariantViewModelBaseModel } from '@umbraco-cms/backend-api';

export class UmbVariantId {
	culture: string | null;
	segment: string | null;

	constructor(variantData?: VariantViewModelBaseModel) {
		this.culture = variantData?.culture || null;
		this.segment = variantData?.segment || null;
	}

	public compare(obj: { culture?: string | null; segment?: string | null }): boolean {
		return this.culture === (obj.culture || null) && this.segment === (obj.segment || null);
	}
}
