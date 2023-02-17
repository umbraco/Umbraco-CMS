import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { DocumentPropertyModel, PropertyTypeContainerViewModelBaseModel } from '@umbraco-cms/backend-api';
import './document-workspace-view-edit-properties.element';

@customElement('umb-document-workspace-view-edit-tab')
export class UmbDocumentWorkspaceViewEditTabElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			uui-box {
				margin-bottom: var(--uui-size-layout-1);
			}
		`,
	];

	private _tabName?: string | undefined;

	@property({ type: String })
	public get tabName(): string | undefined {
		return this._tabName;
	}
	public set tabName(value: string | undefined) {
		const oldValue = this._tabName;
		if (oldValue === value) return;
		this._tabName = value;
		this._observeTabContainers();
		this.requestUpdate('tabName', oldValue);
	}

	@state()
	_tabContainers: PropertyTypeContainerViewModelBaseModel[] = [];

	@state()
	_groupContainersMap: Map<string, Array<PropertyTypeContainerViewModelBaseModel>> = new Map();

	@state()
	_propertyStructure: DocumentPropertyModel[] = [];

	@state()
	_propertyValues: DocumentPropertyModel[] = [];

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
		if (!this._workspaceContext || !this._tabName) return;

		this.observe(
			this._workspaceContext.containersByNameAndType(this._tabName, 'Tab'),
			(tabContainers) => {
				this._tabContainers = tabContainers || [];
				this._observeGroups();
			},
			'_observeTabContainers'
		);
	}

	private _observeGroups() {
		if (!this._workspaceContext || !this._tabName) return;

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
						}
					});
				},
				'_observeGroupsOf_' + container.key
			);
		});
	}

	render() {
		// TODO: only show tab properties if there was any. We need some event to tell us when the properties is empty.
		return html`
			<uui-box>
				<umb-document-workspace-view-edit-properties
					container-type="Tab"
					container-name=${this.tabName || ''}></umb-document-workspace-view-edit-properties>
			</uui-box>
			${repeat(
				this._groupContainersMap,
				(mapEntry) => mapEntry[0],
				(mapEntry) => html`<uui-box .headline=${mapEntry[0]}>
					<umb-document-workspace-view-edit-properties
						container-type="Group"
						container-name=${mapEntry[0]}></umb-document-workspace-view-edit-properties>
				</uui-box>`
			)}
		`;
	}
}

export default UmbDocumentWorkspaceViewEditTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-tab': UmbDocumentWorkspaceViewEditTabElement;
	}
}
