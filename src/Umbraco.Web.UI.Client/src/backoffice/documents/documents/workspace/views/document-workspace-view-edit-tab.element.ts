import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { DocumentPropertyModel, PropertyTypeContainerViewModelBaseModel } from '@umbraco-cms/backend-api';

@customElement('umb-document-workspace-view-edit-tab')
export class UmbDocumentWorkspaceViewEditTabElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];

	private _tabName?: string | undefined;

	@property({ type: String })
	public get tabName(): string | undefined {
		return this._tabName;
	}
	public set tabName(value: string | undefined) {
		if (this._tabName === value) return;
		this._tabName = value;
		this._observeTabContainers();
	}

	@state()
	_tabContainers: PropertyTypeContainerViewModelBaseModel[] = [];

	@state()
	_groupContainersMap: Map<string, Array<PropertyTypeContainerViewModelBaseModel>> = new Map();

	@state()
	_propertyStructure: DocumentPropertyModel[] = [];

	@state()
	_propertyValue: DocumentPropertyModel[] = [];

	//_propertiesObservables: Map<string, unknown> = new Map();

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbDocumentWorkspaceContext>('umbWorkspaceContext', (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			this._observeTabContainers();
		});
	}

	private _observeTabContainers() {
		if (!this._workspaceContext || !this.tabName) return;

		this.observe(
			this._workspaceContext.containersByNameAndType(this.tabName, 'Tab'),
			(tabContainers) => {
				this._tabContainers = tabContainers || [];
				this._observeGroups();
			},
			'_observeTabContainers'
		);
	}

	private _observeGroups() {
		if (!this._workspaceContext || !this.tabName) return;

		this._tabContainers.forEach((container) => {
			this.observe(
				this._workspaceContext!.containersOfParentKey(container.key, 'Group'),
				(groupContainers) => {
					groupContainers.forEach((group) => {
						if (group.name) {
							let groups: PropertyTypeContainerViewModelBaseModel[];
							if (!this._groupContainersMap.has(group.name)) {
								groups = [];
								this._groupContainersMap.set(group.name, groups);
							} else {
								groups = this._groupContainersMap.get(group.name)!;
							}
							groups.push(group);
							// Gather property aliases of this group, by group key.
							this._observePropertyStructureOfGroup(group);
						}
					});
				},
				'_observeGroupsOf_' + container.key
			);
		});
	}

	private _observePropertyStructureOfGroup(group: PropertyTypeContainerViewModelBaseModel) {
		if (!this._workspaceContext || !group.key) return undefined;

		this.observe(
			this._workspaceContext.propertyStructuresOf(group.key),
			(properties) => {
				this._propertyStructure = properties || [];
			},
			'_observePropertyStructureOfGroup' + group.key
		);

		// cache observable
	}

	render() {
		return repeat(
			this._groupContainersMap,
			(mapEntry) => mapEntry[0],
			(mapEntry) => html` <uui-box>${mapEntry[0]}</uui-box> `
		);

		/*
								${repeat(
									this._propertyStructure.filter((property) => property.groupKey === group.key,
									(property) => property.alias,
									(property) =>
										html`<umb-content-property
											.property=${property}
											.value=${this._propertyValue.find((x) => x.alias === property.alias)?.value}></umb-content-property> `
								)}
								*/
	}
}

export default UmbDocumentWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-tab': UmbDocumentWorkspaceViewEditTabElement;
	}
}
