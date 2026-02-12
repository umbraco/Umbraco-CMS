import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDataTypeDetailRepository, type UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { ManifestWorkspaceView, UmbEntityWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbContentTypeModel, UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

type partialManifestWorkspaceView = Omit<Partial<ManifestWorkspaceView>, 'meta'> & {
	meta: Partial<ManifestWorkspaceView['meta']>;
};

export class UmbContentCollectionManager<
	ContentTypeDetailModelType extends UmbContentTypeModel = UmbContentTypeModel,
> extends UmbControllerBase {
	#host: UmbEntityWorkspaceContext & UmbControllerHost;

	#collectionAlias?: string;

	#collectionConfig = new UmbObjectState<UmbCollectionConfiguration | undefined>(undefined);
	readonly collectionConfig = this.#collectionConfig.asObservable();

	#manifestOverrides = new UmbObjectState<partialManifestWorkspaceView | undefined>(undefined);
	readonly manifestOverrides = this.#manifestOverrides.asObservable();

	#hasCollection = new UmbBooleanState(false);
	readonly hasCollection = this.#hasCollection.asObservable();

	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	constructor(
		host: UmbEntityWorkspaceContext & UmbControllerHost,
		structureManager: UmbContentTypeStructureManager<ContentTypeDetailModelType>,
		collectionAlias?: string,
	) {
		super(host);

		this.#host = host;
		this.#collectionAlias = collectionAlias;

		this.observe(
			collectionAlias ? structureManager.ownerContentType : undefined,
			async (contentType) => {
				this.#hasCollection.setValue(!!contentType?.collection);

				const dataTypeUnique = contentType?.collection?.unique;
				if (dataTypeUnique) {
					this.#dataTypeDetailRepository.requestByUnique(dataTypeUnique);
					this.observe(
						await this.#dataTypeDetailRepository.byUnique(dataTypeUnique),
						(dataType) => {
							this.#gotDataType(dataType);
						},
						'_observeConfigDataType',
					);
				}
			},
			null,
		);
	}

	getCollectionAlias() {
		return this.#collectionAlias;
	}

	#gotDataType(dataType?: UmbDataTypeDetailModel): void {
		if (!dataType) {
			this.#collectionConfig.setValue(undefined);
			this.#manifestOverrides.setValue(undefined);
			return;
		}

		const config = new UmbPropertyEditorConfigCollection(dataType.values);
		const pageSize = Number(config.getValueByAlias('pageSize'));

		this.#collectionConfig.setValue({
			unique: this.#host.getUnique(),
			layouts: config?.getValueByAlias('layouts'),
			orderBy: config?.getValueByAlias('orderBy') ?? 'updateDate',
			orderDirection: config?.getValueByAlias('orderDirection') ?? 'asc',
			pageSize: isNaN(pageSize) ? 50 : pageSize,
			userDefinedProperties: config?.getValueByAlias('includeProperties'),
		});

		const overrides: partialManifestWorkspaceView = {
			alias: 'Umb.WorkspaceView.Content.Collection',
			meta: {},
		};

		const overrideIcon = config?.getValueByAlias<string | undefined>('icon');
		if (overrideIcon && overrideIcon !== '') {
			overrides.meta!.icon = overrideIcon;
		}

		const overrideLabel = config?.getValueByAlias<string | undefined>('tabName');
		if (overrideLabel && overrideLabel !== '') {
			overrides.meta!.label = overrideLabel;
		}

		const showFirst = config?.getValueByAlias<boolean | undefined>('showContentFirst');
		if (showFirst === true) {
			overrides.weight = 150;
		}

		this.#manifestOverrides.setValue(overrides);
	}
}
