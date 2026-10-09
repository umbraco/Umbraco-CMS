import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPropertyDatasetElement, UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import type { PropertyEditorSettingsProperty, UmbStartNodeAccessValue } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-user-group-workspace-assign-access')
export class UmbUserGroupWorkspaceAssignAccessElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _values: Array<UmbPropertyValueData> = [
		{ alias: 'sections', value: [] },
		{ alias: 'languageAccess', value: { rootAccess: false, startNodes: [] } },
		{ alias: 'documentAccess', value: { rootAccess: false, startNodes: [] } },
		{ alias: 'mediaAccess', value: { rootAccess: false, startNodes: [] } },
	];

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			if (!instance) return;

			this.observe(instance.sections, (value) => this.#setValue('sections', value ?? []), '_observeSections');

			this.observe(
				observeMultiple([instance.hasAccessToAllLanguages, instance.languages]),
				([rootAccess, languages]) =>
					this.#setValue('languageAccess', {
						rootAccess: rootAccess ?? false,
						startNodes: (languages ?? []).map((unique) => ({ unique })),
					}),
				'_observeLanguageAccess',
			);

			this.observe(
				observeMultiple([instance.documentRootAccess, instance.documentStartNode]),
				([rootAccess, startNode]) =>
					this.#setValue('documentAccess', {
						rootAccess: rootAccess ?? false,
						startNodes: startNode ? [{ unique: startNode.unique }] : [],
					}),
				'_observeDocumentAccess',
			);

			this.observe(
				observeMultiple([instance.mediaRootAccess, instance.mediaStartNode]),
				([rootAccess, startNode]) =>
					this.#setValue('mediaAccess', {
						rootAccess: rootAccess ?? false,
						startNodes: startNode ? [{ unique: startNode.unique }] : [],
					}),
				'_observeMediaAccess',
			);
		});
	}

	#setValue(alias: string, value: unknown) {
		this._values = this._values.map((entry) => (entry.alias === alias ? { alias, value } : entry));
	}

	#onChange(event: Event & { target: UmbPropertyDatasetElement }) {
		const values = event.target.value;

		const sections = values.find((entry) => entry.alias === 'sections')?.value as Array<string> | undefined;
		this.#workspaceContext?.setSections(sections ?? []);

		const languageAccess = values.find((entry) => entry.alias === 'languageAccess')?.value as
			| UmbStartNodeAccessValue
			| undefined;
		if (languageAccess) this.#workspaceContext?.setLanguageAccess(languageAccess);

		const documentAccess = values.find((entry) => entry.alias === 'documentAccess')?.value as
			| UmbStartNodeAccessValue
			| undefined;
		if (documentAccess) this.#workspaceContext?.setDocumentAccess(documentAccess);

		const mediaAccess = values.find((entry) => entry.alias === 'mediaAccess')?.value as
			| UmbStartNodeAccessValue
			| undefined;
		if (mediaAccess) this.#workspaceContext?.setMediaAccess(mediaAccess);
	}

	#getFields(): Array<PropertyEditorSettingsProperty> {
		return [
			{
				alias: 'sections',
				label: this.localize.term('main_sections'),
				description: this.localize.term('user_sectionsHelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.SectionPicker',
			},
			{
				alias: 'languageAccess',
				label: this.localize.term('treeHeaders_languages'),
				description: this.localize.term('user_languagesHelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.LanguageAccess',
				config: [{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllLanguages') }],
			},
			{
				alias: 'documentAccess',
				label: this.localize.term('defaultdialogs_selectContentStartNode'),
				description: this.localize.term('user_startnodehelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentStartNodeAccess',
				config: [
					{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllDocuments') },
					{ alias: 'validationLimit', value: { min: 0, max: 1 } },
				],
			},
			{
				alias: 'mediaAccess',
				label: this.localize.term('defaultdialogs_selectMediaStartNode'),
				description: this.localize.term('user_mediastartnodehelp'),
				propertyEditorUiAlias: 'Umb.PropertyEditorUi.MediaStartNodeAccess',
				config: [
					{ alias: 'rootAccessLabel', value: this.localize.term('user_allowAccessToAllMedia') },
					{ alias: 'validationLimit', value: { min: 0, max: 1 } },
				],
			},
		];
	}

	override render() {
		return html`
			<uui-box .headline=${this.localize.term('user_assignAccess')}>
				<umb-property-dataset .value=${this._values} @change=${this.#onChange}>
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
				</umb-property-dataset>
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
		'umb-user-group-workspace-assign-access': UmbUserGroupWorkspaceAssignAccessElement;
	}
}
