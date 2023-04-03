import { UmbDocumentWorkspaceContext } from '../../../../documents/documents/workspace/document-workspace.context';
import { PropertyContainerTypes } from './workspace-structure-manager.class';
import { DocumentTypePropertyTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController, UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';
import { ArrayState, UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

export class UmbWorkspacePropertyStructureHelper {
	#host: UmbControllerHostElement;

	#workspaceContext?: UmbDocumentWorkspaceContext;

	private _containerType?: PropertyContainerTypes;
	private _isRoot?: boolean;
	private _containerName?: string;

	#propertyStructure = new ArrayState<DocumentTypePropertyTypeResponseModel>([], (x) => x.key);
	readonly propertyStructure = this.#propertyStructure.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		new UmbContextConsumerController(host, UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context as UmbDocumentWorkspaceContext;
			this._observeGroupContainers();
		});
	}

	public setContainerType(value?: PropertyContainerTypes) {
		if (this._containerType === value) return;
		this._containerType = value;
		this._observeGroupContainers();
	}
	public getContainerType() {
		return this._containerType;
	}

	public setContainerName(value?: string) {
		if (this._containerName === value) return;
		this._containerName = value;
		this._observeGroupContainers();
	}
	public getContainerName() {
		return this._containerName;
	}

	public setIsRoot(value: boolean) {
		if (this._isRoot === value) return;
		this._isRoot = value;
		this._observeGroupContainers();
	}
	public getIsRoot() {
		return this._isRoot;
	}

	private _observeGroupContainers() {
		if (!this.#workspaceContext || !this._containerType) return;

		if (this._isRoot === true) {
			this._observePropertyStructureOf(null);
		} else if (this._containerName !== undefined) {
			new UmbObserverController(
				this.#host,
				this.#workspaceContext!.structure.containersByNameAndType(this._containerName, this._containerType),
				(groupContainers) => {
					groupContainers.forEach((group) => this._observePropertyStructureOf(group.key));
				},
				'_observeGroupContainers'
			);
		}
	}

	private _observePropertyStructureOf(groupKey?: string | null) {
		if (!this.#workspaceContext || groupKey === undefined) return;

		new UmbObserverController(
			this.#host,
			this.#workspaceContext.structure.propertyStructuresOf(groupKey),
			(properties) => {
				// If this need to be able to remove properties, we need to clean out the ones of this group.key before inserting them:
				const _propertyStructure = this.#propertyStructure.getValue().filter((x) => x.containerId !== groupKey);

				properties?.forEach((property) => {
					if (!_propertyStructure.find((x) => x.alias === property.alias)) {
						_propertyStructure.push(property);
					}
				});

				if (_propertyStructure.length > 0) {
					// TODO: End-point: Missing sort order?
					//_propertyStructure = _propertyStructure.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
				}

				// Fire update to subscribers:
				this.#propertyStructure.next(_propertyStructure);
			},
			'_observePropertyStructureOfGroup' + groupKey
		);
	}

	/** Manipulate methods: */

	async addProperty(ownerKey?: string, sortOrder?: number) {
		if (!this.#workspaceContext) return;

		return await this.#workspaceContext.structure.createProperty(null, ownerKey, sortOrder);
	}

	// Takes optional arguments as this is easier for the implementation in the view:
	async partialUpdateProperty(propertyKey?: string, partialUpdate?: Partial<DocumentTypePropertyTypeResponseModel>) {
		if (!this.#workspaceContext || !propertyKey || !partialUpdate) return;

		return await this.#workspaceContext.structure.updateProperty(null, propertyKey, partialUpdate);
	}
}
