import { UmbUfmRenderElement } from '../components/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Renders a UFM
 */
export class UmbUfmVirtualRenderController extends UmbControllerBase {
	#element: UmbUfmRenderElement;

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
		this.#element.markdown = markdown;
	}
	get markdown(): string | undefined {
		return this.#element.markdown;
	}

	set value(value: unknown | undefined) {
		this.#element.value = value;
	}
	get value(): unknown | undefined {
		return this.#element.value;
	}

	constructor(host: UmbControllerHost) {
		super(host);

		const element = new UmbUfmRenderElement();
		element.inline = true;
		element.style.visibility = 'hidden';
		this.getHostElement().appendChild(element);
		this.#element = element;
	}

	override hostConnected(): void {}

	override toString(): string {
		return this.#getTextFromDescendants(this.#element);
	}

	override destroy(): void {
		super.destroy();
		this.#element?.destroy();
		(this.#element as any) = undefined;
	}
}
