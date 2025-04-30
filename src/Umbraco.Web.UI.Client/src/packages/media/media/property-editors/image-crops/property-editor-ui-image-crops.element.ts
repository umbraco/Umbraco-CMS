import { html, customElement, property, css, repeat, state, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export type UmbCrop = {
	label: string;
	alias: string;
	width: number;
	height: number;
};

@customElement('umb-property-editor-ui-image-crops')
export class UmbPropertyEditorUIImageCropsElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@query('#label')
	private _labelInput!: HTMLInputElement;

	@state()
	private _value: Array<UmbCrop> = [];

	@state()
	private _isCreating = false;

	@property({ type: Array })
	public set value(value: Array<UmbCrop>) {
		this._value = value ?? [];
		this.#sorter.setModel(this.value);
	}
	public get value(): Array<UmbCrop> {
		return this._value;
	}

	@state()
	editCropAlias = '';

	#oldInputValue = '';

	#sorter = new UmbSorterController(this, {
		getUniqueOfElement: (element: HTMLElement) => element.dataset['alias'],
		getUniqueOfModel: (modelEntry: UmbCrop) => modelEntry.alias,
		identifier: 'Umb.SorterIdentifier.ImageCrops',
		itemSelector: '.crop',
		containerSelector: '.crops',
		onChange: ({ model }) => {
			const oldValue = this._value;
			this._value = model;
			this.requestUpdate('_value', oldValue);
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	#onRemove(alias: string) {
		this.value = [...this.value.filter((item) => item.alias !== alias)];
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onEdit(crop: UmbCrop) {
		this.editCropAlias = crop.alias;
		this._isCreating = false;
	}

	#onEditCancel() {
		this.editCropAlias = '';
		this._isCreating = false;
	}

	#onSubmit(event: Event) {
		event.preventDefault();
		const form = event.target as HTMLFormElement;
		if (!form || !form.checkValidity()) return;

		const formData = new FormData(form);
		const label = formData.get('label') as string;
		const alias = formData.get('alias') as string;
		const width = formData.get('width') as string;
		const height = formData.get('height') as string;

		if (!label || !alias || !width || !height) return;

		const newCrop: UmbCrop = {
			label,
			alias,
			width: parseInt(width),
			height: parseInt(height),
		};

		if (this.editCropAlias) {
			const index = this.value.findIndex((item) => item.alias === this.editCropAlias);
			if (index === -1) return;
			const temp = [...this.value];
			temp[index] = newCrop;
			this.value = [...temp];
			this.editCropAlias = '';
		} else {
			this.value = [...this.value, newCrop];
		}

		this.dispatchEvent(new UmbChangeEvent());
		form.reset();
		this._labelInput?.focus();
		this._isCreating = false;
	}

	#renderForm(initial?: UmbCrop) {
		return html`
			<uui-form>
				<form @submit=${this.#onSubmit}>
					<div class="input">
						<uui-label for="label">Label</uui-label>
						<uui-input
							@input=${this.#onLabelInput}
							label="Label"
							id="label"
							name="label"
							type="text"
							.value=${initial?.label ?? ''}></uui-input>
					</div>
					<div class="input">
						<uui-label for="alias">Alias</uui-label>
						<uui-input label="Alias" id="alias" name="alias" type="text" autocomplete="false" .value=${initial?.alias ?? ''}></uui-input>
					</div>
					<div class="input">
						<uui-label for="width">Width</uui-label>
						<uui-input label="Width" id="width" name="width" type="number" autocomplete="false" .value=${initial?.width ?? ''} min="0">
							<span class="append" slot="append">px</span>
						</uui-input>
					</div>
					<div class="input">
						<uui-label for="height">Height</uui-label>
						<uui-input label="Height" id="height" name="height" type="number" autocomplete="false" .value=${initial?.height ?? ''} min="0">
							<span class="append" slot="append">px</span>
						</uui-input>
					</div>
					<div class="action-wrapper">
						${this.editCropAlias
				? html`<uui-button @click=${this.#onEditCancel}>Cancel</uui-button>
								   <uui-button look="secondary" type="submit" label="Save"></uui-button>`
				: html`<uui-button look="secondary" type="submit" label="Add"></uui-button>`}
					</div>
				</form>
			</uui-form>
		`;
	}

	#onLabelInput() {
		const value = this._labelInput.value ?? '';
		const aliasValue = generateAlias(value);
		const alias = this.shadowRoot?.querySelector('#alias') as HTMLInputElement;
		if (!alias) return;

		const oldAliasValue = generateAlias(this.#oldInputValue);
		if (alias.value === oldAliasValue || !alias.value) {
			alias.value = aliasValue;
		}

		this.#oldInputValue = value;
	}

	override render() {
		return html`
			<uui-ref-list class="crops">
	${repeat(
			this.value,
			(item) => item.alias,
			(item) => html`
			${this.editCropAlias === item.alias
					? html`<div class="crop-form">${this.#renderForm(item)}</div>`
					: html`
					<uui-ref-node
						class="crop"
						data-alias="${item.alias}"
						detail="${item.width} x ${item.height}px"
						name="${item.label} (${item.alias})">
						<uui-icon slot="icon" name="icon-crop"></uui-icon>
						<uui-action-bar slot="actions">
							<uui-button
								label=${this.localize.term('general_edit')}
								color="default"
								@click=${() => this.#onEdit(item)}></uui-button>
							<uui-button
								label=${this.localize.term('general_remove')}
								color="danger"
								@click=${() => this.#onRemove(item.alias)}></uui-button>
						</uui-action-bar>
					</uui-ref-node>
				`}
		`
		)}
</uui-ref-list>
	${!this._isCreating && !this.editCropAlias
				? html`<uui-button look="outline" @click=${() => (this._isCreating = true)}>Create crop</uui-button>`
				: ''}
			${this._isCreating ? this.#renderForm() : ''}
		`;
	}
	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}
			.crops {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
				margin-top: var(--uui-size-space-4);
			}
			.crop {
				display: flex;
				align-items: center;
			}
			form {
				display: flex;
				gap: var(--uui-size-space-2);
				flex-wrap: wrap;
			}
			.input {
				display: flex;
				flex-direction: column;
				flex: 1 1 200px;
			}
			uui-input[type='number'] {
				text-align: right;
			}
			.append {
				padding-inline: var(--uui-size-space-4);
				background: var(--uui-color-disabled);
				border-left: 1px solid var(--uui-color-border);
				color: var(--uui-color-disabled-contrast);
				font-size: var(--uui-type-small-size);
				display: flex;
				align-items: center;
			}
			.action-wrapper {
				display: flex;
				align-items: flex-end;
			}
		`,
	];
}

export default UmbPropertyEditorUIImageCropsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-crops': UmbPropertyEditorUIImageCropsElement;
	}
}