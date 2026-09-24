import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import { css, customElement, html, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbPropertyDatasetContextBase } from '@umbraco-cms/backoffice/property';
import type { PropertyEditorSettingsProperty, UmbStartNodeAccessValue } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-user-workspace-assign-access')
export class UmbUserWorkspaceAssignAccessElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	readonly #dataset = new UmbPropertyDatasetContextBase(this);

	constructor() {
		super();

		this.#observeDatasetProperty<Array<UmbReferenceByUnique>>('userGroups', (value) =>
			this.#workspaceContext?.setUserGroups(value ?? []),
		);
		this.#observeDatasetProperty<UmbStartNodeAccessValue>('documentAccess', (value) =>
			value ? this.#workspaceContext?.setDocumentAccess(value) : undefined,
		);
		this.#observeDatasetProperty<UmbStartNodeAccessValue>('mediaAccess', (value) =>
			value ? this.#workspaceContext?.setMediaAccess(value) : undefined,
		);
		this.#observeDatasetProperty<UmbStartNodeAccessValue>('elementAccess', (value) =>
			value ? this.#workspaceContext?.setElementAccess(value) : undefined,
		);

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			if (!instance) return;

			this.observe(
				instance.userGroupUniques,
				(value) => this.#dataset.setPropertyValue('userGroups', value ?? []),
				'_observeUserGroupAccess',
			);

			this.observe(
				observeMultiple([instance.hasDocumentRootAccess, instance.documentStartNodeUniques]),
				([rootAccess, startNodes]) =>
					this.#dataset.setPropertyValue('documentAccess', {
						rootAccess: rootAccess ?? false,
						startNodes: startNodes ?? [],
					}),
				'_observeDocumentAccess',
			);

			this.observe(
				observeMultiple([instance.hasMediaRootAccess, instance.mediaStartNodeUniques]),
				([rootAccess, startNodes]) =>
					this.#dataset.setPropertyValue('mediaAccess', {
						rootAccess: rootAccess ?? false,
						startNodes: startNodes ?? [],
					}),
				'_observeMediaAccess',
			);
			this.observe(
				observeMultiple([instance.hasElementRootAccess, instance.elementStartNodeUniques]),
				([rootAccess, startNodes]) =>
					this.#dataset.setPropertyValue('elementAccess', {
						rootAccess: rootAccess ?? false,
						startNodes: startNodes ?? [],
					}),
				'_observeElementAccess',
			);
		});
	}

	#observeDatasetProperty<ValueType>(alias: string, callback: (value: ValueType | undefined) => void) {
		this.#dataset.propertyValueByAlias<ValueType>(alias).then((valueSource) => {
			this.observe(valueSource, callback, `_observeDatasetProperty_${alias}`);
		});
	}

	#getFields(): Array<PropertyEditorSettingsProperty> {
		return [
			{
				alias: 'userGroups',
				label: this.localize.term('general_groups'),
				description: this.localize.term('user_groupsHelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.UserGroupPicker',
			},
			{
				alias: 'documentAccess',
				label: this.localize.term('user_startnodes'),
				description: this.localize.term('user_startnodeshelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentStartNodeAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllDocuments') }],
			},
			{
				alias: 'mediaAccess',
				label: this.localize.term('defaultdialogs_selectMediaStartNode'),
				description: this.localize.term('user_mediastartnodehelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaStartNodeAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllMedia') }],
			},
			{
				alias: 'elementAccess',
				label: this.localize.term('user_selectElementStartNode'),
				description: this.localize.term('user_selectElementStartNodeDescription'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.ElementStartNodeAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllElements') }],
			},
		];
	}

	override render() {
		return html`
			<uui-box .headline=${this.localize.term('user_assignAccess')}>
				${repeat(
					this.#getFields(),
					(field) => field.alias,
					(field) => html`
						<umb-property
							alias=${field.alias}
							label=${field.label}
							description=${ifDefined(field.description)}
							property-editor-ui-alias=${field.propertyEditorUiAlias}
							.config=${field.config}>
						</umb-property>
					`,
				)}
			</uui-box>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-assign-access': UmbUserWorkspaceAssignAccessElement;
	}
}
