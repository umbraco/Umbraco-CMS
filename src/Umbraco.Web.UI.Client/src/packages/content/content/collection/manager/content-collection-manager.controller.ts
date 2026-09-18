import { UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT } from '../configuration/content-collection-configuration.context-token.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { ManifestWorkspaceView, UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';

type partialManifestWorkspaceView = Omit<Partial<ManifestWorkspaceView>, 'meta'> & {
	meta: Partial<ManifestWorkspaceView['meta']>;
};

/**
 * @deprecated Deprecated since v18. Use `UmbContentCollectionConfigurationContext` instead, which resolves the same
 * configuration for any host rather than only a workspace. Scheduled for removal in Umbraco 20.
 */
export class UmbContentCollectionManager<
	ContentTypeDetailModelType extends UmbContentTypeModel = UmbContentTypeModel,
> extends UmbControllerBase {
	#collectionAlias?: string;

	#collectionConfig = new UmbObjectState<UmbCollectionConfiguration | undefined>(undefined);

	/**
	 * The resolved collection configuration.
	 * @returns {Observable<UmbCollectionConfiguration | undefined>} The resolved collection configuration.
	 * @deprecated Deprecated since v18. Observe `collectionConfig` on
	 * `UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT` instead. Scheduled for removal in Umbraco 20.
	 */
	get collectionConfig() {
		new UmbDeprecation({
			removeInVersion: '20.0.0',
			deprecated: 'UmbContentCollectionManager.collectionConfig',
			solution: 'Observe `collectionConfig` on UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT instead.',
		}).warn();
		return this.#collectionConfig.asObservable();
	}

	#manifestOverrides = new UmbObjectState<partialManifestWorkspaceView | undefined>(undefined);
	readonly manifestOverrides = this.#manifestOverrides.asObservable();

	#hasCollection = new UmbBooleanState(false);
	readonly hasCollection = this.#hasCollection.asObservable();

	/**
	 * @param {UmbControllerHost} host - The workspace this manager is bound to.
	 * @param {UmbContentTypeStructureManager} _structureManager - Retained for backwards compatibility. The collection
	 * configuration is now resolved by `UmbContentCollectionConfigurationContext`, which the workspace feeds.
	 * @param {string} [collectionAlias] - The alias of the collection to render.
	 */
	constructor(
		host: UmbEntityWorkspaceContext & UmbControllerHost,
		_structureManager: UmbContentTypeStructureManager<ContentTypeDetailModelType>,
		collectionAlias?: string,
	) {
		super(host);

		this.#collectionAlias = collectionAlias;

		this.consumeContext(UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT, (context) => {
			this.observe(context?.collectionConfig, (config) => this.#collectionConfig.setValue(config), null);
			this.observe(context?.hasCollection, (hasCollection) => this.#hasCollection.setValue(!!hasCollection), null);
			// The manifest override is a workspace-view concern, so it is derived here from what the context resolved
			// rather than inside a context that knows nothing about workspace views.
			this.observe(context?.dataType, (dataType) => this.#setManifestOverrides(dataType), null);
		});
	}

	getCollectionAlias() {
		return this.#collectionAlias;
	}

	#setManifestOverrides(dataType?: UmbDataTypeDetailModel): void {
		if (!dataType) {
			this.#manifestOverrides.setValue(undefined);
			return;
		}

		const config = new UmbPropertyEditorConfigCollection(dataType.values);

		const overrides: partialManifestWorkspaceView = {
			alias: 'Umb.WorkspaceView.Content.Collection',
			meta: {},
		};

		const overrideIcon = config.getValueByAlias<string | undefined>('icon');
		if (overrideIcon && overrideIcon !== '') {
			overrides.meta!.icon = overrideIcon;
		}

		const overrideLabel = config.getValueByAlias<string | undefined>('tabName');
		if (overrideLabel && overrideLabel !== '') {
			overrides.meta!.label = overrideLabel;
		}

		const showFirst = config.getValueByAlias<boolean | undefined>('showContentFirst');
		if (showFirst === true) {
			overrides.weight = 150;
		}

		this.#manifestOverrides.setValue(overrides);
	}
}
