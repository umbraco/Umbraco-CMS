import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import {
	DocumentPropertyModel,
	DocumentTypePropertyTypeModel,
	PropertyTypeContainerViewModelBaseModel,
} from '@umbraco-cms/backend-api';

@customElement('umb-document-workspace-view-edit-properties')
export class UmbDocumentWorkspaceViewEditPropertiesElement extends UmbLitElement {
	static styles = [UUITextStyles];

	private _containerName?: string;

	@property({ type: String, attribute: 'container-name', reflect: false })
	public get containerName(): string | undefined {
		return this._containerName;
	}
	public set containerName(value: string | undefined) {
		if (this._containerName === value) return;
		this._containerName = value;
		this._observeGroupContainers();
	}

	private _containerType?: 'Group' | 'Tab';

	@property({ type: String, attribute: 'container-type', reflect: false })
	public get containerType(): 'Group' | 'Tab' | undefined {
		return this._containerType;
	}
	public set containerType(value: 'Group' | 'Tab' | undefined) {
		if (this._containerType === value) return;
		this._containerType = value;
		this._observeGroupContainers();
	}

	@state()
	_groupContainers: Array<PropertyTypeContainerViewModelBaseModel> = [];

	@state()
	_propertyStructure: Array<DocumentTypePropertyTypeModel> = [];

	@state()
	_propertyValueMap: Map<string, DocumentPropertyModel> = new Map();

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbDocumentWorkspaceContext>('umbWorkspaceContext', (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			this._observeGroupContainers();
		});
	}

	private _observeGroupContainers() {
		if (!this._workspaceContext || !this._containerName || !this._containerType) return;

		// TODO: Should be no need to update this observable if its already there.
		this.observe(
			this._workspaceContext!.containersByNameAndType(this._containerName, this._containerType),
			(groupContainers) => {
				this._groupContainers = groupContainers || [];
				groupContainers.forEach((group) => {
					if (group.key) {
						// Gather property aliases of this group, by group key.
						this._observePropertyStructureOfGroup(group);
					}
				});
			},
			'_observeGroupContainers'
		);
	}

	private _observePropertyStructureOfGroup(group: PropertyTypeContainerViewModelBaseModel) {
		if (!this._workspaceContext || !group.key) return;

		// TODO: Should be no need to update this observable if its already there.
		this.observe(
			this._workspaceContext.propertyStructuresOf(group.key),
			(properties) => {
				// If this need to be able to remove properties, we need to clean out the ones of this group.key before inserting them:
				this._propertyStructure = this._propertyStructure.filter((x) => x.containerKey !== group.key);

				properties?.forEach((property) => {
					if (!this._propertyStructure.find((x) => x.alias === property.alias)) {
						this._propertyStructure.push(property);
						this._observePropertyValueOfAlias(property.alias!);
					}
				});

				if (this._propertyStructure.length > 0) {
					// TODO: Missing sort order?
					//this._propertyStructure.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
				}
			},
			'_observePropertyStructureOfGroup' + group.key
		);

		// cache observable
	}

	private _observePropertyValueOfAlias(propertyAlias: string) {
		if (!this._workspaceContext || !propertyAlias) return;

		// TODO: Should be no need to update this observable if its already there.
		this.observe(
			this._workspaceContext.propertyValueOfAlias(propertyAlias, null, null),
			(propertyValue) => {
				if (propertyValue) {
					this._propertyValueMap.set(propertyAlias, propertyValue);
				} else {
					this._propertyValueMap.delete(propertyAlias);
				}
			},
			'_observePropertyValueOfAlias' + propertyAlias
		);
	}

	render() {
		return repeat(
			this._propertyStructure,
			(property) => property.alias,
			(property) =>
				html`<umb-content-property
					.property=${property}
					.value=${this._propertyValueMap.get(property.alias!)?.value}></umb-content-property> `
		);
	}
}

export default UmbDocumentWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-properties': UmbDocumentWorkspaceViewEditPropertiesElement;
	}
}
