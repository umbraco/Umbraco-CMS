import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { UUISelectEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../modal.service';
import { UmbModalLayoutElement } from '../modal-layout.element';

@customElement('umb-modal-layout-property-settings')
export class UmbModalLayoutPropertySettingsElement extends UmbModalLayoutElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				color: var(--uui-color-text);
			}
			#content {
				padding: var(--uui-size-space-6);
			}
			#appearances {
				display: flex;
				gap: var(--uui-size-space-6);
				max-width: 350px;
				margin: 0 auto;
			}
			.appearance {
				position: relative;
				display: flex;
				border: 2px solid var(--uui-color-border-standalone);
				padding: 0 16px;
				align-items: center;
				border-radius: 6px;
				opacity: 0.8;
			}
			.appearance:not(.selected):hover {
				border-color: var(--uui-color-border-emphasis);
				cursor: pointer;
				opacity: 1;
			}
			.appearance.selected {
				border-color: var(--uui-color-selected);
				opacity: 1;
			}
			.appearance.selected::after {
				content: '';
				position: absolute;
				inset: 0;
				border-radius: 6px;
				opacity: 0.1;
				background-color: var(--uui-color-selected);
			}
			.appearance.left {
				flex-grow: 1;
			}
			.appearance.top {
				flex-shrink: 1;
			}
			.appearance svg {
				display: flex;
				width: 100%;
				color: var(--uui-color-text);
			}
			hr {
				border: none;
				border-top: 1px solid var(--uui-color-divider);
				margin-top: var(--uui-size-space-6);
				margin-bottom: var(--uui-size-space-5);
			}
			uui-input {
				width: 100%;
			}
			#alias-lock {
				display: flex;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}
			#alias-lock uui-icon {
				margin-bottom: 2px;
			}
			#property-editor-ui-picker {
				width: 100%;
				--uui-button-padding-top-factor: 4;
				--uui-button-padding-bottom-factor: 4;
			}
			.container {
				display: flex;
				flex-direction: column;
			}
		`,
	];

	@state() private _propertyEditorUIIcon = '';
	@state() private _propertyEditorUIName = '';
	@state() private _propertyEditorUiAlias = '';
	@state() private _propertyEditorAlias = '';

	@state() private _appearanceIsLeft = true;

	@state() private _customValidationOptions = [
		{
			name: 'No validation',
			value: 'no-validation',
			selected: true,
		},
		{
			name: 'Validate as an email address',
			value: 'email',
			validation: '[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+',
		},
		{
			name: 'Validate as a number',
			value: 'number',
			validation: '^[0-9]*$',
		},
		{
			name: 'Validate as an URL',
			value: 'url',
			validation: 'https?://[a-zA-Z0-9-.]+.[a-zA-Z]{2,}',
		},
		{
			name: '...or enter a custom validation',
			value: 'custom',
		},
	];
	@state() private _customValidation = this._customValidationOptions[0];
	@state() private _customValidationErrorMessage = '';

	@state() private _aliasLocked = true;

	#modalService?: UmbModalService;

	/**
	 *
	 */
	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});

		this.#observePropertyEditorUI();
	}

	#observePropertyEditorUI() {
		if (!this._propertyEditorUiAlias) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', this._propertyEditorUiAlias),
			(propertyEditorUI) => {
				if (!propertyEditorUI) return;

				this._propertyEditorUIName = propertyEditorUI?.meta.label ?? propertyEditorUI?.name ?? '';
				this._propertyEditorUiAlias = propertyEditorUI?.alias ?? '';
				this._propertyEditorUIIcon = propertyEditorUI?.meta.icon ?? '';
				this._propertyEditorAlias = propertyEditorUI?.meta.propertyEditorModel ?? '';
			}
		);
	}

	#onCustomValidationChange(event: UUISelectEvent) {
		const value = event.target.value;

		this._customValidation =
			this._customValidationOptions.find((option) => option.value === value) ?? this._customValidationOptions[0];

		console.log('custom validation changed to: ', this._customValidationOptions);
	}

	#onClose() {
		this.modalHandler?.close();
	}

	#onSubmit() {
		this.modalHandler?.close();
	}

	#onAppearanceChange(event: MouseEvent) {
		const target = event.target as HTMLElement;
		const alreadySelected = target.classList.contains(this._appearanceIsLeft ? 'left' : 'top');

		if (alreadySelected) return;

		this._appearanceIsLeft = !this._appearanceIsLeft;

		console.log('appearance changed to: ', this._appearanceIsLeft ? 'left' : 'top');
	}

	#onOpenPropertyEditorUIPicker() {
		const modalHandler = this.#modalService?.propertyEditorUIPicker({
			selection: [],
		});

		if (!modalHandler) return;

		modalHandler?.onClose().then(({ selection } = {}) => {
			if (!selection) return;

			this._propertyEditorUiAlias = selection[0];
			this.#observePropertyEditorUI();
		});
	}

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
	}

	render() {
		return html` <umb-workspace-layout headline="Property settings">
			<div id="content">
				<uui-box>
					<div class="container">
						<uui-input id="name-input" placeholder="Enter a name..."> </uui-input>
						<uui-input id="alias-input" placeholder="Enter alias..." ?disabled=${this._aliasLocked}>
							<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
								<uui-icon name=${this._aliasLocked ? 'umb:lock' : 'umb:unlocked'}></uui-icon>
							</div>
						</uui-input>
						<uui-textarea id="description-input" placeholder="Enter description..."></uui-textarea>
					</div>
					${this.#renderPropertyUIPicker()}
					<hr />
					<div class="container">
						<b>Validation</b>
						<div style="display: flex; justify-content: space-between">
							<label for="mandatory">Field is mandatory</label>
							<uui-toggle id="mandatory" slot="editor"></uui-toggle>
						</div>
						<p style="margin-bottom: 0">Custom validation</p>
						${this.#renderCustomValidation()}
					</div>
					<hr />
					<div class="container">
						<b style="margin-bottom: var(--uui-size-space-3)">Appearance</b>
						<div id="appearances">${this.#renderLeftSVG()} ${this.#renderTopSVG()}</div>
					</div>
				</uui-box>
			</div>
			<div slot="actions">
				<uui-button label="Close" @click=${this.#onClose}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#onSubmit}></uui-button>
			</div>
		</umb-workspace-layout>`;
	}

	#renderLeftSVG() {
		return html`<div
			@click=${this.#onAppearanceChange}
			@keydown=${() => ''}
			class="appearance left ${this._appearanceIsLeft ? 'selected' : ''}">
			<svg width="260" height="60" viewBox="0 0 260 60" fill="none" xmlns="http://www.w3.org/2000/svg">
				<rect width="89" height="14" rx="7" fill="currentColor" />
				<rect x="121" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				<rect x="121" y="46" width="108" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				<rect x="121" y="23" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
			</svg>
		</div>`;
	}

	#renderTopSVG() {
		return html`
			<div
				@click=${this.#onAppearanceChange}
				@keydown=${() => ''}
				class="appearance top ${this._appearanceIsLeft ? '' : 'selected'}">
				<svg width="139" height="90" viewBox="0 0 139 90" fill="none" xmlns="http://www.w3.org/2000/svg">
					<rect width="89" height="14" rx="7" fill="currentColor" />
					<rect y="30" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
					<rect y="76" width="108" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
					<rect y="53" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				</svg>
			</div>
		`;
	}

	#renderPropertyUIPicker() {
		return this._propertyEditorUiAlias
			? html`
					<umb-ref-property-editor-ui
						name=${this._propertyEditorUIName}
						alias=${this._propertyEditorUiAlias}
						property-editor-model-alias=${this._propertyEditorAlias}
						border>
						<uui-icon name="${this._propertyEditorUIIcon}" slot="icon"></uui-icon>
						<uui-action-bar slot="actions">
							<uui-button label="Change" @click=${this.#onOpenPropertyEditorUIPicker}></uui-button>
						</uui-action-bar>
					</umb-ref-property-editor-ui>
			  `
			: html`
					<uui-button
						id="property-editor-ui-picker"
						label="Select Property Editor"
						look="placeholder"
						color="default"
						@click=${this.#onOpenPropertyEditorUIPicker}></uui-button>
			  `;
	}

	#renderCustomValidation() {
		return html`<uui-select
				@change=${this.#onCustomValidationChange}
				.options=${this._customValidationOptions}></uui-select>

			${this._customValidation.value !== 'no-validation'
				? html`
						<uui-input
							style="margin-bottom: var(--uui-size-space-1); margin-top: var(--uui-size-space-5);"
							value=${this._customValidation.validation ?? ''}></uui-input>
						<uui-textarea value=${this._customValidationErrorMessage}></uui-textarea>
				  `
				: nothing} `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-property-settings': UmbModalLayoutPropertySettingsElement;
	}
}
