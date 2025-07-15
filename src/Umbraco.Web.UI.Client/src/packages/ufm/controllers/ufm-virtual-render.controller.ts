import { UmbUfmRenderElement } from '../components/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Renders a UFM
 */
export class UmbUfmVirtualRenderController extends UmbControllerBase {
	#element?: UmbUfmRenderElement;

	#getTextFromDescendants(element?: Element | null): string {
		if (!element) return '';

		const items: Array<string> = [];

		items.push(element.shadowRoot?.textContent ?? element.textContent ?? '');

		if (element.shadowRoot !== null) {
			Array.from(element.shadowRoot.children).forEach((element) => {
				items.push(this.#getTextFromDescendants(element));
			});
		}

		if (element.children !== null) {
			Array.from(element.children).forEach((element) => {
				items.push(this.#getTextFromDescendants(element));
			});
		}

		return items.filter((x) => x).join(' ');
	}

	set markdown(markdown: string | undefined) {
		this.#markdown = markdown;
		if (this.#element) {
			this.#element.markdown = markdown;
		}
	}
	get markdown(): string | undefined {
		return this.#markdown;
	}
	#markdown: string | undefined;

	set value(value: unknown | undefined) {
		this.#value = value;
		if (this.#element) {
			this.#element.value = value;
		}
	}
	get value(): unknown | undefined {
		return this.#value;
	}
	#value: unknown | undefined;

	override hostConnected(): void {
		const element = new UmbUfmRenderElement();
		element.inline = true;
		element.style.visibility = 'hidden';

		element.markdown = this.#markdown;
		element.value = this.#value;

		this.getHostElement().appendChild(element);
		this.#element = element;
	}

	override hostDisconnected(): void {
		this.#element?.remove();
	}

	override toString(): string {
		return this.#getTextFromDescendants(this.#element);
	}

	override destroy(): void {
		super.destroy();
		this.#element?.destroy();
		(this.#element as any) = undefined;
	}
}
