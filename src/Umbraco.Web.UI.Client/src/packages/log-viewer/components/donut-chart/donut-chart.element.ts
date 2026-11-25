import type { UmbDonutSliceElement } from './donut-slice.element.js';
import { umbFormatDonutShare } from './format-donut-share.function.js';
import { umbLayoutDonutSlices } from './layout-donut-slices.function.js';
import type { UmbDonutSliceLayout } from './layout-donut-slices.function.js';
import {
	classMap,
	css,
	customElement,
	html,
	nothing,
	property,
	query,
	queryAssignedElements,
	state,
	styleMap,
	svg,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

interface UmbDonutChartSliceData {
	amount: number;
	color: string;
	name: string;
	kind: string;
	href: string;
	label: string;
}

interface UmbDonutChartSegment extends UmbDonutChartSliceData, UmbDonutSliceLayout {
	path: string;
}

interface UmbDonutChartGeometry {
	size: number;
	radius: number;
	innerRadius: number;
}

const DEFAULT_SVG_SIZE = 100;
const DEFAULT_RADIUS = 45;
const DEFAULT_BORDER_SIZE = 20;
const SLICE_GAP_DEGREES = 1.5;
const DETAILS_BOX_OFFSET = 12;

const roundCoordinate = (value: number): number => Math.round(value * 1000) / 1000;

const placeWithin = (anchor: number, length: number, available: number): number => {
	const after = anchor + DETAILS_BOX_OFFSET;
	const preferred = after + length <= available ? after : anchor - DETAILS_BOX_OFFSET - length;
	return Math.min(Math.max(preferred, 0), Math.max(available - length, 0));
};

/**
 * A donut chart that shows each slotted `umb-donut-slice` as a share of the total of all slices.
 *
 * Each `umb-donut-slice` takes an `amount`, a `color` (any CSS colour, including `var(...)`), a `name`, a `kind` (the
 * unit shown after the amount) and optionally an `href` with a `label` for the link's accessible name. Hovering,
 * focusing or tapping a slice shows its name, amount, kind and share. Set the element's width to size the chart (it
 * defaults to 200px); the height follows. Consumers with many small categories should group the long tail themselves,
 * for example into an "Other" slice.
 * @element umb-donut-chart
 * @slot - The `umb-donut-slice` elements to show.
 * @cssprop --uui-color-border - The ring shown when no slice has an amount, and the details box border.
 * @cssprop --uui-color-surface - The details box background, and the inner edge of the focus ring.
 * @cssprop --uui-color-text - The details box text.
 * @cssprop --uui-color-text-alt - The description text.
 * @cssprop --uui-color-focus - The focus ring.
 * @cssprop --uui-shadow-depth-3 - The details box shadow.
 * @cssprop --uui-border-radius - The details box corners.
 * @cssprop --uui-size-space-2 - Spacing in the details box and above the description.
 * @cssprop --uui-size-space-3 - Spacing in the details box.
 * @cssprop --uui-type-small-size - The details box and description text size.
 */
@customElement('umb-donut-chart')
export class UmbDonutChartElement extends UmbLitElement {
	/**
	 * Converts a percentage of the full circle to degrees.
	 * @param {number} percent - The percentage of the full circle.
	 * @returns {number} The angle in degrees.
	 */
	static percentToDegrees(percent: number): number {
		return percent * 3.6;
	}

	/**
	 * The default for `minSlicePercent`: the smallest share of the ring, in percent, that a slice with an amount is
	 * drawn with, so it stays visible and can be hovered.
	 */
	static MIN_SLICE_PERCENT = 1;

	/**
	 * Circle radius in SVG user units (see `svgSize`)
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number })
	radius = DEFAULT_RADIUS;

	/**
	 * The circle thickness in SVG user units. Clamped to `radius`.
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'border-size' })
	borderSize = DEFAULT_BORDER_SIZE;

	/**
	 * The width and height of the SVG viewBox in user units. The rendered size follows the element's width.
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'svg-size' })
	svgSize = DEFAULT_SVG_SIZE;

	/**
	 * Description of the graph, used as the accessible name of the list of slices
	 * @memberof UmbDonutChartElement
	 */
	@property()
	description = '';

	/**
	 * Hides the box that appears on hover or keyboard focus with the details of the slice
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Boolean })
	hideDetailBox = false;

	/**
	 * Shows the description text below the chart
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Boolean, attribute: 'show-description' })
	showDescription = false;

	/**
	 * The smallest share of the ring, in percent, that a slice with an amount is drawn with. It shrinks when there are
	 * too many slices for each to get it. Defaults to `UmbDonutChartElement.MIN_SLICE_PERCENT`. The details box and
	 * accessible names always use the true share.
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'min-slice-percent' })
	minSlicePercent?: number;

	@queryAssignedElements({ selector: 'umb-donut-slice', flatten: true })
	private _sliceElements!: UmbDonutSliceElement[];

	@query('#container')
	private _container!: HTMLDivElement;

	@query('#details-box')
	private _detailsBox?: HTMLDivElement;

	@state()
	private _activeIndex?: number;

	@state()
	private _focusIndex?: number;

	#slices: Array<UmbDonutChartSliceData> = [];
	#slicesChanged = false;
	#geometry: UmbDonutChartGeometry = {
		size: DEFAULT_SVG_SIZE,
		radius: DEFAULT_RADIUS,
		innerRadius: DEFAULT_RADIUS - DEFAULT_BORDER_SIZE,
	};
	#segments: Array<UmbDonutChartSegment> = [];
	#minSlicePercent?: number;
	#detailsIndex?: number;
	#detailsBoxAnchor = { x: 0, y: 0 };

	#onSlicesChange = (event: Event) => {
		event.stopPropagation();
		this.#slicesChanged = true;
		this.requestUpdate();
	};

	#onPointerOver = (event: PointerEvent) => {
		if (event.pointerType === 'touch') return;
		this._activeIndex = this.#getSegmentIndex(event.target);
		this.#setDetailsBoxAnchorFromPointer(event);
	};

	#onPointerMove = (event: PointerEvent) => {
		if (event.pointerType === 'touch' || this._activeIndex === undefined) return;
		this.#setDetailsBoxAnchorFromPointer(event);
		this.#positionDetailsBox();
	};

	#onPointerLeave = (event: PointerEvent) => {
		if (event.pointerType === 'touch') return;
		this._activeIndex = this._focusIndex;
		if (this._activeIndex !== undefined) this.#setDetailsBoxAnchorFromSegment(this._activeIndex);
	};

	#onPointerUp = (event: PointerEvent) => {
		if (event.pointerType !== 'touch') return;
		const index = this.#getSegmentIndex(event.target);
		if (index !== undefined && this.#segments[index]?.href) return;
		this._activeIndex = index === this._activeIndex ? undefined : index;
		if (this._activeIndex !== undefined) this.#setDetailsBoxAnchorFromPointer(event);
	};

	#onFocusIn = (event: FocusEvent) => {
		const target = event.target as Element;
		const index = this.#getSegmentIndex(target);
		if (index === undefined || !target.matches(':focus-visible')) return;
		this._focusIndex = index;
		this._activeIndex = index;
		this.#setDetailsBoxAnchorFromSegment(index);
	};

	#onFocusOut = () => {
		this._focusIndex = undefined;
		this._activeIndex = undefined;
	};

	#onDocumentKeyDown = (event: KeyboardEvent) => {
		if (event.key === 'Escape') this._activeIndex = undefined;
	};

	#onDocumentPointerDown = (event: PointerEvent) => {
		if (this._activeIndex !== undefined && !event.composedPath().includes(this)) this._activeIndex = undefined;
	};

	override connectedCallback(): void {
		super.connectedCallback();
		document.addEventListener('keydown', this.#onDocumentKeyDown);
		document.addEventListener('pointerdown', this.#onDocumentPointerDown);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		document.removeEventListener('keydown', this.#onDocumentKeyDown);
		document.removeEventListener('pointerdown', this.#onDocumentPointerDown);
		this._activeIndex = undefined;
		this._focusIndex = undefined;
	}

	protected override willUpdate(changedProperties: Map<PropertyKey, unknown>): void {
		super.willUpdate(changedProperties);

		if (this.#updateLayoutInputs(changedProperties)) {
			this.#updateSegments();
		}

		if (this._activeIndex !== undefined) {
			this.#detailsIndex = this._activeIndex;
		}
	}

	#updateLayoutInputs(changedProperties: Map<PropertyKey, unknown>): boolean {
		const geometryChanged = ['radius', 'borderSize', 'svgSize'].some((name) => changedProperties.has(name));
		if (geometryChanged) {
			this.#geometry = this.#resolveGeometry();
		}

		const minSlicePercentChanged = changedProperties.has('minSlicePercent');
		if (minSlicePercentChanged) {
			this.#minSlicePercent = this.#resolveMinSlicePercent();
		}

		const slicesChanged = this.#readChangedSlices();
		return geometryChanged || minSlicePercentChanged || slicesChanged;
	}

	#readChangedSlices(): boolean {
		if (!this.#slicesChanged) return false;
		this.#slicesChanged = false;
		this.#slices = this._sliceElements.map(({ amount, color, name, kind, href, label }) => ({
			amount,
			color,
			name,
			kind,
			href,
			label,
		}));
		return true;
	}

	#updateSegments(): void {
		this.#segments = this.#layoutSegments();
		this._activeIndex = this.#existingSegmentIndex(this._activeIndex);
		this._focusIndex = this.#existingSegmentIndex(this._focusIndex);
	}

	#existingSegmentIndex(index: number | undefined): number | undefined {
		return index !== undefined && index < this.#segments.length ? index : undefined;
	}

	protected override updated(changedProperties: Map<PropertyKey, unknown>): void {
		super.updated(changedProperties);
		if (this._activeIndex !== undefined) {
			this.#positionDetailsBox();
		}
	}

	#resolveGeometry(): UmbDonutChartGeometry {
		const size = this.#positiveOr(this.svgSize, DEFAULT_SVG_SIZE, 'svg-size');

		let radius = this.#positiveOr(this.radius, DEFAULT_RADIUS, 'radius');
		if (radius > size / 2) {
			console.warn(`umb-donut-chart: radius (${radius}) does not fit svg-size (${size}). Using ${size / 2}.`);
			radius = size / 2;
		}

		let borderSize = this.#positiveOr(this.borderSize, DEFAULT_BORDER_SIZE, 'border-size');
		if (borderSize > radius) {
			console.warn(`umb-donut-chart: border-size (${borderSize}) is larger than radius (${radius}). Using ${radius}.`);
			borderSize = radius;
		}

		return { size, radius, innerRadius: radius - borderSize };
	}

	#positiveOr(value: number, fallback: number, name: string): number {
		if (Number.isFinite(value) && value > 0) return value;
		console.warn(`umb-donut-chart: ${name} must be a positive number, got ${value}. Using ${fallback}.`);
		return fallback;
	}

	#resolveMinSlicePercent(): number | undefined {
		const value = this.minSlicePercent;
		if (value === undefined || value === null) return undefined;
		if (Number.isFinite(value) && value >= 0) return value;
		console.warn(
			`umb-donut-chart: min-slice-percent must be zero or a positive number, got ${value}. Using ${UmbDonutChartElement.MIN_SLICE_PERCENT}.`,
		);
		return undefined;
	}

	#layoutSegments(): Array<UmbDonutChartSegment> {
		const minSlicePercent = this.#minSlicePercent ?? UmbDonutChartElement.MIN_SLICE_PERCENT;
		const layout = umbLayoutDonutSlices(
			this.#slices.map((slice) => slice.amount),
			{ minSweep: UmbDonutChartElement.percentToDegrees(minSlicePercent), gap: SLICE_GAP_DEGREES },
		);
		return layout.map((slice) => ({ ...this.#slices[slice.index], ...slice, path: this.#getSlicePath(slice) }));
	}

	#getSlicePath({ startAngle, sweep }: UmbDonutSliceLayout): string {
		if (sweep >= 360) return this.#getRingPath();

		const { radius, innerRadius } = this.#geometry;
		const endAngle = startAngle + sweep;
		const largeArc = sweep > 180 ? 1 : 0;
		return [
			`M ${this.#getPoint(startAngle, radius)}`,
			`A ${radius} ${radius} 0 ${largeArc} 1 ${this.#getPoint(endAngle, radius)}`,
			`L ${this.#getPoint(endAngle, innerRadius)}`,
			`A ${innerRadius} ${innerRadius} 0 ${largeArc} 0 ${this.#getPoint(startAngle, innerRadius)}`,
			'Z',
		].join(' ');
	}

	#getRingPath(): string {
		const { radius, innerRadius } = this.#geometry;
		const circle = (r: number, sweepFlag: number) =>
			`M ${this.#getPoint(0, r)} A ${r} ${r} 0 1 ${sweepFlag} ${this.#getPoint(180, r)} A ${r} ${r} 0 1 ${sweepFlag} ${this.#getPoint(0, r)} Z`;
		return `${circle(radius, 1)} ${circle(innerRadius, 0)}`;
	}

	#getPoint(angle: number, distance: number): string {
		const { x, y } = this.#getCoordinates(angle, distance);
		return `${roundCoordinate(x)} ${roundCoordinate(y)}`;
	}

	#getCoordinates(angle: number, distance: number) {
		const center = this.#geometry.size / 2;
		const radians = (angle * Math.PI) / 180;
		return { x: center + distance * Math.sin(radians), y: center - distance * Math.cos(radians) };
	}

	#getSegmentIndex(target: EventTarget | null): number | undefined {
		const index = target instanceof Element ? target.closest('[data-index]')?.getAttribute('data-index') : null;
		return index ? Number(index) : undefined;
	}

	#setDetailsBoxAnchorFromPointer(event: PointerEvent) {
		const bounds = this._container.getBoundingClientRect();
		this.#detailsBoxAnchor = { x: event.clientX - bounds.left, y: event.clientY - bounds.top };
	}

	#setDetailsBoxAnchorFromSegment(index: number) {
		const segment = this.#segments[index];
		if (!segment) return;
		const { size, radius, innerRadius } = this.#geometry;
		const scale = this._container.clientWidth / size;
		const { x, y } = this.#getCoordinates(segment.startAngle + segment.sweep / 2, (radius + innerRadius) / 2);
		this.#detailsBoxAnchor = { x: x * scale, y: y * scale };
	}

	#positionDetailsBox() {
		if (!this._detailsBox) return;
		const { x, y } = this.#detailsBoxAnchor;
		const left = placeWithin(x, this._detailsBox.offsetWidth, this._container.clientWidth);
		const top = placeWithin(y, this._detailsBox.offsetHeight, this._container.clientHeight);
		this._detailsBox.style.translate = `${left}px ${top}px`;
	}

	#getDetails(segment: UmbDonutChartSegment): string {
		const amount = [this.localize.number(segment.amount), segment.kind].filter(Boolean).join(' ');
		return `${amount} (${umbFormatDonutShare(segment.share, this.localize)})`;
	}

	#getAccessibleName(segment: UmbDonutChartSegment): string {
		const details = this.#getDetails(segment);
		return segment.name ? `${segment.name}: ${details}` : details;
	}

	#renderSegment(segment: UmbDonutChartSegment, index: number) {
		const name = this.#getAccessibleName(segment);
		const slice = svg`<path
			class="slice"
			fill-rule="evenodd"
			d=${segment.path}
			style=${styleMap({ fill: segment.color })}
			role=${segment.href ? nothing : 'img'}
			aria-label=${segment.href ? nothing : name}></path>`;

		return svg`<g
			role="listitem"
			class=${classMap({ segment: true, active: index === this._activeIndex })}
			data-index=${index}>
			${
				segment.href
					? svg`<a href=${segment.href} aria-label=${segment.label || name}>
						${segment.label ? svg`<desc>${name}</desc>` : nothing}${slice}
					</a>`
					: slice
			}
		</g>`;
	}

	#renderFocusRing() {
		const segment = this._focusIndex === undefined ? undefined : this.#segments[this._focusIndex];
		if (!segment) return nothing;
		return svg`<g class="focus-ring" aria-hidden="true">
			<path class="focus-ring-outer" d=${segment.path}></path>
			<path class="focus-ring-inner" d=${segment.path}></path>
		</g>`;
	}

	#renderChart() {
		const { size } = this.#geometry;
		if (this.#segments.length === 0) {
			return html`<svg
				viewBox="0 0 ${size} ${size}"
				role=${this.description ? 'img' : nothing}
				aria-label=${this.description || nothing}
				aria-hidden=${this.description ? nothing : 'true'}>
				<path class="track" fill-rule="evenodd" d=${this.#getRingPath()}></path>
			</svg>`;
		}

		return html`<svg
			viewBox="0 0 ${size} ${size}"
			role="list"
			aria-label=${this.description || nothing}
			class=${classMap({ 'has-active': this._activeIndex !== undefined })}
			@pointerover=${this.#onPointerOver}
			@pointermove=${this.#onPointerMove}
			@pointerleave=${this.#onPointerLeave}
			@pointerup=${this.#onPointerUp}>
			${this.#segments.map((segment, index) => this.#renderSegment(segment, index))}${this.#renderFocusRing()}
		</svg>`;
	}

	#renderDetailsBox() {
		if (this.hideDetailBox) return nothing;
		const segment = this.#detailsIndex === undefined ? undefined : this.#segments[this.#detailsIndex];
		return html`<div id="details-box" class=${classMap({ show: this._activeIndex !== undefined })} aria-hidden="true">
			${segment
				? html`<div id="details-title">
							<span class="swatch" style=${styleMap({ background: segment.color })}></span>${segment.name}
						</div>
						<div>${this.#getDetails(segment)}</div>`
				: nothing}
		</div>`;
	}

	override render() {
		return html`<div id="container" @focusin=${this.#onFocusIn} @focusout=${this.#onFocusOut}>
				${this.#renderChart()}${this.#renderDetailsBox()}
			</div>
			${this.showDescription && this.description ? html`<p class="description">${this.description}</p>` : nothing}
			<slot @slotchange=${this.#onSlicesChange} @slice-update=${this.#onSlicesChange}></slot>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 200px;
				max-width: 100%;
			}

			#container {
				position: relative;
				aspect-ratio: 1;
			}

			svg {
				display: block;
				width: 100%;
				height: 100%;
				overflow: visible;
			}

			.track {
				fill: var(--uui-color-border);
			}

			.segment {
				transition: opacity 120ms linear;
			}

			svg.has-active .segment:not(.active) {
				opacity: 0.7;
			}

			a {
				outline: none;
			}

			.focus-ring path {
				fill: none;
				pointer-events: none;
				stroke-linejoin: round;
				vector-effect: non-scaling-stroke;
			}

			.focus-ring-outer {
				stroke: var(--uui-color-surface);
				stroke-width: 6px;
			}

			.focus-ring-inner {
				stroke: var(--uui-color-focus);
				stroke-width: 2px;
			}

			#details-box {
				position: absolute;
				top: 0;
				left: 0;
				z-index: 1;
				box-sizing: border-box;
				width: max-content;
				max-width: 100%;
				padding: var(--uui-size-space-2) var(--uui-size-space-3);
				background: var(--uui-color-surface);
				color: var(--uui-color-text);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
				font-size: var(--uui-type-small-size);
				line-height: 1.5;
				overflow-wrap: anywhere;
				opacity: 0;
				pointer-events: none;
				transition: opacity 120ms linear;
			}

			#details-box.show {
				opacity: 1;
			}

			#details-title {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
				font-weight: bold;
			}

			.swatch {
				flex: none;
				width: 0.75em;
				height: 0.75em;
				border-radius: 50%;
				forced-color-adjust: none;
			}

			.description {
				text-align: center;
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
				margin: var(--uui-size-space-2) 0 0 0;
			}

			@media (prefers-reduced-motion: reduce) {
				.segment,
				#details-box {
					transition: none;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-chart': UmbDonutChartElement;
	}
}
