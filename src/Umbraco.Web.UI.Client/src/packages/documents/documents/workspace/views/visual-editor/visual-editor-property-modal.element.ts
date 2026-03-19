import type {
	UmbVisualEditorPropertyGroup,
	UmbVisualEditorPropertyInfo,
	UmbVisualEditorPropertyModalData,
	UmbVisualEditorPropertyModalValue,
} from './visual-editor-property-modal.token.js';
import { css, customElement, html, ifDefined, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * A sidebar modal that renders one or more property editors using
 * `<umb-property-dataset>` + `<umb-property>` for the full Umbraco editing experience.
 * Used for both single document properties and block content properties.
 * When the block has a settings element type, a separate "Settings" section is rendered.
 */
@customElement('umb-visual-editor-property-modal')
export class UmbVisualEditorPropertyModalElement extends UmbModalBaseElement<
	UmbVisualEditorPropertyModalData,
	UmbVisualEditorPropertyModalValue
> {
	@state() private _values: Array<{ alias: string; value: unknown }> = [];
	@state() private _settingsValues: Array<{ alias: string; value: unknown }> = [];
	@state() private _activeTab: 'content' | 'settings' = 'content';

	#validationContext = new UmbValidationContext(this);

	override connectedCallback() {
		super.connectedCallback();
		if (this.data) {
			this._values = [...this.data.values];
			this._settingsValues = [...(this.data.settingsValues ?? [])];
		}
	}

	#onDatasetChange(e: UmbChangeEvent) {
		const dataset = e.target as HTMLElement & { value: Array<{ alias: string; value: unknown }> };
		this._values = dataset.value ?? [];
	}

	#onSettingsDatasetChange(e: UmbChangeEvent) {
		const dataset = e.target as HTMLElement & { value: Array<{ alias: string; value: unknown }> };
		this._settingsValues = dataset.value ?? [];
	}

	async #onSubmit() {
		try {
			await this.#validationContext.validate();
		} catch {
			// Validation failed — messages are shown by the property editors
			return;
		}

		this.modalContext?.setValue({ values: this._values, settingsValues: this._settingsValues });
		this.modalContext?.submit();
	}

	#renderProperties(properties: UmbVisualEditorPropertyInfo[]) {
		return properties.map(
			(prop) => html`
				<umb-property
					label=${prop.name}
					description=${ifDefined(prop.description)}
					alias=${prop.alias}
					property-editor-ui-alias=${prop.editorUiAlias}
					.config=${prop.config}
					.validation=${prop.validation}>
				</umb-property>
			`,
		);
	}

	#renderGroupedProperties(
		properties: UmbVisualEditorPropertyInfo[],
		groups: UmbVisualEditorPropertyGroup[] | undefined,
		values: Array<{ alias: string; value: unknown }>,
		onDatasetChange: (e: UmbChangeEvent) => void,
	) {
		// Properties without a container (root-level)
		const rootProps = properties.filter((p) => !p.containerId);
		// Group the rest by containerId
		const grouped = (groups ?? []).map((group) => ({
			...group,
			properties: properties.filter((p) => p.containerId === group.id),
		})).filter((g) => g.properties.length > 0);

		const hasGroups = grouped.length > 0;

		return html`
			<umb-property-dataset .value=${values} @change=${onDatasetChange}>
				${rootProps.length > 0
					? html`<uui-box>${this.#renderProperties(rootProps)}</uui-box>`
					: nothing}
				${hasGroups
					? grouped.map(
							(group) => html`
								<uui-box .headline=${group.name}>
									${this.#renderProperties(group.properties)}
								</uui-box>
							`,
						)
					: !rootProps.length
						? html`<uui-box>${this.#renderProperties(properties)}</uui-box>`
						: nothing}
			</umb-property-dataset>
		`;
	}

	#renderContentTab() {
		return this.#renderGroupedProperties(
			this.data!.properties,
			this.data!.groups,
			this._values,
			this.#onDatasetChange.bind(this),
		);
	}

	#renderSettingsTab() {
		return this.#renderGroupedProperties(
			this.data!.settingsProperties!,
			this.data!.settingsGroups,
			this._settingsValues,
			this.#onSettingsDatasetChange.bind(this),
		);
	}

	override render() {
		if (!this.data) return html``;

		const hasSettings = this.data.settingsProperties && this.data.settingsProperties.length > 0;

		return html`
			<umb-body-layout headline=${this.data.headline}>
				${hasSettings
					? html`
							<uui-tab-group slot="navigation">
								<uui-tab
									label="Content"
									.active=${this._activeTab === 'content'}
									@click=${() => (this._activeTab = 'content')}>
									<uui-icon slot="icon" name="icon-document"></uui-icon>
									Content
								</uui-tab>
								<uui-tab
									label="Settings"
									.active=${this._activeTab === 'settings'}
									@click=${() => (this._activeTab = 'settings')}>
									<uui-icon slot="icon" name="icon-settings"></uui-icon>
									Settings
								</uui-tab>
							</uui-tab-group>
						`
					: nothing}
				<div id="editor">
					${hasSettings
						? this._activeTab === 'content'
							? this.#renderContentTab()
							: this.#renderSettingsTab()
						: this.#renderContentTab()}
				</div>
				<div slot="actions">
					<uui-button label="Close" @click=${this._rejectModal}></uui-button>
					<uui-button label="Update" look="primary" color="positive" @click=${this.#onSubmit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}

			uui-box {
				--uui-box-default-padding: 0 var(--uui-size-space-5);
			}
		`,
	];
}

export default UmbVisualEditorPropertyModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-visual-editor-property-modal': UmbVisualEditorPropertyModalElement;
	}
}
