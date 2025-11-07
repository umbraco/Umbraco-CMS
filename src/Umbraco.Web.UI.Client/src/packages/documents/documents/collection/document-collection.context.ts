import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from './types.js';
import { UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS } from './constants.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyValuePresentationDisplayOption, type ManifestPropertyValuePresentation } from 'src/packages/core/property-value-presentation/index.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export type UmbGetPropertyValueByAliasArgs = {
	alias: string;
	documentTypeAlias: string;
	createDate?: Date;
	updateDate?: Date;
	state?: string;
	culture?: string | null | undefined;
	creator?: string | null | undefined;
	updater?: string | null | undefined;
	sortOrder: number;
	values: Array<{ alias: string; editorAlias: string; culture?: string; segment?: string; value: string }>;
}

export class UmbDocumentCollectionContext extends UmbDefaultCollectionContext<
	UmbDocumentCollectionItemModel,
	UmbDocumentCollectionFilterModel
> {
	#variantContext?: typeof UMB_VARIANT_CONTEXT.TYPE;
	#displayCulture = new UmbStringState(undefined);
	#displayCultureObservable = this.#displayCulture.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS);

		this.consumeContext(UMB_VARIANT_CONTEXT, async (variantContext) => {
			this.#variantContext = variantContext;
			this.#observeDisplayCulture();
		});
	}

	#observeDisplayCulture() {
		this.observe(
			this.#variantContext?.displayCulture,
			(displayCulture) => {
				if (!displayCulture) return;
				if (this.#displayCulture.getValue() === displayCulture) return;
				this.#displayCulture.setValue(displayCulture);
				this.setFilter({ orderCulture: displayCulture });
			},
			'umbDocumentCollectionDisplayCultureObserver',
		);
	}

	/**
	 * Requests the collection from the repository.
	 * @returns {Promise<void>}
	 * @deprecated Deprecated since v.17.0.0. Use `loadCollection` instead.
	 * @memberof UmbDocumentCollectionContext
	 */
	public override async requestCollection(): Promise<void> {
		new UmbDeprecation({
			removeInVersion: '19.0.0',
			deprecated: 'requestCollection',
			solution: 'Use .loadCollection method instead',
		}).warn();

		return this._requestCollection();
	}

	protected override async _requestCollection() {
		await this.observe(this.#displayCultureObservable)?.asPromise();
		await super._requestCollection();
	}

	getPropertyValueByAlias(args: UmbGetPropertyValueByAliasArgs) {
		const alias = args.alias;
		switch (alias) {
			case 'contentTypeAlias':
				return args.documentTypeAlias;
			case 'createDate':
				return args.createDate?.toLocaleString();
			case 'creator':
			case 'owner':
				return args.creator;
			case 'published':
				return args.state !== DocumentVariantStateModel.DRAFT ? 'True' : 'False';
			case 'sortOrder':
				return args.sortOrder;
			case 'updateDate':
				return args.updateDate?.toLocaleString();
			case 'updater':
				return args.updater;
			default: {
				const prop = args.values.find((x) => x.alias === alias && (!x.culture || x.culture === args.culture));

				if (prop) {
					const value = prop.value ?? '';
					const propertyValuePresentationManifest = this.#getPropertyValuePresentationManifest(prop.editorAlias);
					if (propertyValuePresentationManifest.length > 0) {
						return html`<umb-extension-slot
							type="propertyValuePresentation"
							.filter=${(x: ManifestPropertyValuePresentation) => x.propertyEditorAlias === prop.editorAlias}
							.props=${{ alias: alias, value: value, display: UmbPropertyValuePresentationDisplayOption.COLLECTION_COLUMN }}
						>
						</umb-extension-slot>`;
					}

					return value;
				}

				return '';
			}
		}
	}

	#getPropertyValuePresentationManifest(propertyEditorAlias: string) {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValuePresentation',
			(manifest) => manifest.propertyEditorAlias === propertyEditorAlias,
		);
	}
}

export { UmbDocumentCollectionContext as api };
