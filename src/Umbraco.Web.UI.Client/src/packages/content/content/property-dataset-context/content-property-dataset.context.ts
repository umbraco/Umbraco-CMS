import type { UmbContentWorkspaceContext } from '../workspace/index.js';
import type { UmbContentDetailModel } from '../types.js';
import { UmbElementPropertyDatasetContext } from './element-property-dataset.context.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityVariantModel, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbRoutePathAddendumContext } from '@umbraco-cms/backoffice/router';

export class UmbContentPropertyDatasetContext<
	ContentModel extends UmbContentDetailModel = UmbContentDetailModel,
	ContentTypeModel extends UmbContentTypeModel = UmbContentTypeModel,
	VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
> extends UmbElementPropertyDatasetContext<
	ContentModel,
	ContentTypeModel,
	UmbContentWorkspaceContext<ContentModel, ContentTypeModel, VariantModelType>
> {
	//
	#pathAddendum = new UmbRoutePathAddendumContext(this);
	#currentVariant = new UmbObjectState<VariantModelType | undefined>(undefined);
	currentVariant = this.#currentVariant.asObservable();

	name = this.#currentVariant.asObservablePart((x) => x?.name);
	culture = this.#currentVariant.asObservablePart((x) => x?.culture);
	segment = this.#currentVariant.asObservablePart((x) => x?.segment);

	readonly IS_CONTENT = true;

	getName(): string | undefined {
		return this._dataOwner.getName(this.getVariantId());
	}
	setName(name: string) {
		this._dataOwner.setName(name, this.getVariantId());
	}
	/**
	 * @deprecated Its not clear why we have this. We should either document the need better or get rid of it.
	 * @returns {UmbEntityVariantModel | undefined} - gives information about the current variant.
	 */
	getVariantInfo() {
		return this._dataOwner.getVariant(this.getVariantId());
	}

	constructor(
		host: UmbControllerHost,
		dataOwner: UmbContentWorkspaceContext<ContentModel, ContentTypeModel, VariantModelType>,
		variantId?: UmbVariantId,
	) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, dataOwner, variantId);

		this.#pathAddendum.setAddendum(variantId ? variantId.toString() : '');

		this.observe(
			this._dataOwner.variantById(this.getVariantId()),
			async (variantInfo) => {
				if (!variantInfo) return;
				this.#currentVariant.setValue(variantInfo);
			},
			null,
		);
	}
}
