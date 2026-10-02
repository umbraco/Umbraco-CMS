import { ExampleBlockViewBase } from './block-view-base.element.js';
import { exampleBlockViewStyles } from './block-view.styles.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';

interface ExampleNestedBlockItem {
	key: string;
	values: Array<{ alias: string; culture?: string | null; value: unknown }>;
}

interface ExampleNestedBlockValue {
	contentData?: Array<ExampleNestedBlockItem>;
	layout?: { 'Umbraco.BlockList'?: Array<{ contentKey: string }> };
}

interface ExampleMaterialSwatch {
	key: string;
	name?: string;
	imageUrl?: string;
}

@customElement('example-material-showcase-block-view')
export class ExampleMaterialShowcaseBlockView extends ExampleBlockViewBase {
	@state()
	private _swatches: Array<ExampleMaterialSwatch> = [];

	#loadId = 0;

	protected override async _load() {
		const nested = this.content?.materials as ExampleNestedBlockValue | undefined;
		const items = nested?.contentData ?? [];
		const layout = nested?.layout?.['Umbraco.BlockList'];

		// The order of a Block List is kept in its layout; contentData keeps the order the blocks were created in.
		const ordered = layout
			? layout.flatMap((entry) => items.find((item) => item.key === entry.contentKey) ?? [])
			: items;

		const loadId = ++this.#loadId;
		const swatches = await Promise.all(
			ordered.map(async (item) => {
				const names = item.values.filter((value) => value.alias === 'material');
				const name = (names.find((value) => value.culture === this._culture) ?? names[0])?.value as string | undefined;
				const image = item.values.find((value) => value.alias === 'image')?.value;

				return { key: item.key, name, imageUrl: await this._requestMediaUrl(image) };
			}),
		);

		if (loadId !== this.#loadId) return;
		this._swatches = swatches;
	}

	override render() {
		return html`
			<div class="body">
				<h2 class="title">${this.content?.headline}</h2>
				${this.content?.intro ? html`<p class="lead">${this.content.intro}</p>` : nothing}
				<div class="swatches">
					${repeat(
						this._swatches,
						(swatch) => swatch.key,
						(swatch) => html`
							<figure>
								<div class="media">${swatch.imageUrl ? html`<img src=${swatch.imageUrl} alt="" />` : nothing}</div>
								<figcaption class="eyebrow">${swatch.name}</figcaption>
							</figure>
						`,
					)}
				</div>
			</div>
		`;
	}

	static override styles = [
		exampleBlockViewStyles,
		css`
			.swatches {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
				gap: var(--uui-size-space-4);
				align-self: stretch;
				margin-top: var(--uui-size-space-3);
			}

			figure {
				margin: 0;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			img {
				aspect-ratio: 1;
			}

			.lead {
				-webkit-line-clamp: unset;
			}
		`,
	];
}

export default ExampleMaterialShowcaseBlockView;

declare global {
	interface HTMLElementTagNameMap {
		'example-material-showcase-block-view': ExampleMaterialShowcaseBlockView;
	}
}
