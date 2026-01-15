import { UmbTiptapToolbarButtonElement } from './tiptap-toolbar-button.element.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import type { UUIColorPickerChangeEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-tiptap-toolbar-color-picker-button')
export class UmbTiptapToolbarColorPickerButtonElement extends UmbTiptapToolbarButtonElement {
	#onChange(event: UUIColorPickerChangeEvent) {
		this._selectedColor = event.target.value;
		this.api?.execute(this.editor, this._selectedColor);
	}

	@state()
	private _selectedColor?: string;

	override render() {
		const label = this.localize.string(this.manifest?.meta.label);
		const disabled = this.api?.isDisabled(this.editor);
		return html`
			<uui-button-group>
				<uui-button
					compact
					label=${label}
					popovertarget=${!this._selectedColor ? 'color-picker-popover' : ''}
					title=${label}
					?disabled=${disabled}
					@click=${() => this.api?.execute(this.editor, this._selectedColor)}>
					<div>
						${when(
							this.manifest?.meta.icon,
							(icon) => html`<umb-icon name=${icon}></umb-icon>`,
							() => html`<span>${label}</span>`,
						)}
						<div id="color-selected" style="background-color:${this._selectedColor ?? '#000'};"></div>
					</div>
				</uui-button>
				<uui-button compact popovertarget="color-picker-popover" label="Open color picker" ?disabled=${disabled}>
					<uui-symbol-expand open></uui-symbol-expand>
				</uui-button>
				<uui-popover-container id="color-picker-popover" placement="bottom-end">
					<umb-popover-layout>
						<uui-scroll-container>
							<uui-color-picker inline label=${label} @change=${this.#onChange}></uui-color-picker>
						</uui-scroll-container>
					</umb-popover-layout>
				</uui-popover-container>
			</uui-button-group>
		`;
	}

	static override readonly styles = [
		css`
			uui-button-group:hover {
				background-color: var(--uui-color-background);
				border-radius: var(--uui-border-radius);
			}

			uui-scroll-container {
				border-radius: var(--uui-border-radius);
				overflow-x: hidden;
			}

			umb-icon {
				height: 1em;
				width: 1em;
				margin-bottom: 1px;
			}

			#color-selected {
				height: var(--uui-size-1);
			}

			uui-button[disabled] {
				#color-selected {
					opacity: 0.3;
				}
			}
		`,
	];
}

export { UmbTiptapToolbarColorPickerButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-color-picker-button': UmbTiptapToolbarColorPickerButtonElement;
	}
}
