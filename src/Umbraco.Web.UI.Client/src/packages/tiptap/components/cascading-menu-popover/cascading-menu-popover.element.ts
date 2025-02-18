import { css, customElement, html, property, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

export type UmbCascadingMenuItem = {
	unique: string;
	label: string;
	icon?: string;
	items?: Array<UmbCascadingMenuItem>;
	element?: HTMLElement;
	separatorAfter?: boolean;
	execute?: () => void;
	isActive?: () => boolean;
};

@customElement('umb-cascading-menu-popover')
export class UmbCascadingMenuPopoverElement extends UUIPopoverContainerElement {
	@property({ type: Array })
	items?: Array<UmbCascadingMenuItem>;

	#getPopoverById(popoverId: string): UUIPopoverContainerElement | null | undefined {
		return this.shadowRoot?.querySelector(`#${popoverId}`) as UUIPopoverContainerElement;
	}

	#onMouseEnter(popoverId: string) {
		const popover = this.#getPopoverById(popoverId);
		if (!popover) return;

		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		popover.showPopover();
	}

	#onMouseLeave(popoverId: string) {
		const popover = this.#getPopoverById(popoverId);
		if (!popover) return;

		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		popover.hidePopover();
	}

	#onClick(item: UmbCascadingMenuItem) {
		item.execute?.();

		setTimeout(() => {
			this.#onMouseLeave(item.unique);
		}, 100);
	}

	override render() {
		return html`
			<uui-scroll-container>
				${when(
					this.items?.length,
					() => html`
						${repeat(
							this.items!,
							(item) => item.unique,
							(item) => this.#renderItem(item),
						)}
						${super.render()}
					`,
					() => super.render(),
				)}
			</uui-scroll-container>
		`;
	}

	#renderItem(item: UmbCascadingMenuItem) {
		const element = item.element;
		if (element) {
			element.setAttribute('popovertarget', item.unique);
		}
		return html`
			<div @mouseenter=${() => this.#onMouseEnter(item.unique)} @mouseleave=${() => this.#onMouseLeave(item.unique)}>
				${when(
					element,
					() => element,
					() => html`
						<uui-menu-item
							popovertarget=${item.unique}
							@click-label=${() => this.#onClick(item)}
							class=${item.separatorAfter ? 'separator' : ''}>
							${when(item.icon, (icon) => html`<uui-icon slot="icon" name=${icon}></uui-icon>`)}
							<div slot="label" class="menu-item">
								<span>${item.label}</span>
								${when(item.items, () => html`<uui-symbol-expand></uui-symbol-expand>`)}
							</div>
						</uui-menu-item>
					`,
				)}
				<umb-cascading-menu-popover id=${item.unique} placement="right-start" .items=${item.items}>
				</umb-cascading-menu-popover>
			</div>
		`;
	}

	static override readonly styles = [
		...UUIPopoverContainerElement.styles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;

				background: var(--uui-color-surface);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
				padding: var(--uui-size-space-1);
			}

			.menu-item {
				flex: 1;
				display: flex;
				justify-content: space-between;
				align-items: center;
				gap: var(--uui-size-space-4);
			}

			.separator::after {
				content: '';
				position: absolute;
				border-bottom: 1px solid var(--uui-color-border);
				width: 100%;
			}

			uui-scroll-container {
				max-height: 500px;
			}
		`,
	];
}

export default UmbCascadingMenuPopoverElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-cascading-menu-popover': UmbCascadingMenuPopoverElement;
	}
}
