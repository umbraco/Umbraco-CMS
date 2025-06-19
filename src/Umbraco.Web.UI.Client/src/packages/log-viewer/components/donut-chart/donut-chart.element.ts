import type { UmbDonutSliceElement } from './donut-slice.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	css,
	html,
	LitElement,
	svg,
	customElement,
	property,
	query,
	queryAssignedElements,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { clamp } from '@umbraco-cms/backoffice/utils';

export interface Circle {
	color: string;
	name: string;
	percent: number;
	kind: string;
	number: number;
}

interface CircleWithCommands extends Circle {
	offset: number;
	commands: string;
}
//TODO: maybe move to UI Library
/**
 * This is a donut chart component that can be used to display data in a circular way.
 * @class UmbDonutChartElement
 * @augments {LitElement}
 */
@customElement('umb-donut-chart')
export class UmbDonutChartElement extends LitElement {
	static percentToDegrees(percent: number): number {
		return percent * 3.6;
	}

	/**
	 * Circle radius in pixels
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number })
	radius = 45;

	/**
	 * The circle thickness in pixels
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'border-size' })
	borderSize = 20;

	/**
	 * The size of SVG element in pixels
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Number, attribute: 'svg-size' })
	svgSize = 100;

	/**
	 * Description of the graph, added for accessibility purposes
	 * @memberof UmbDonutChartElement
	 */
	@property()
	description = '';

	/**
	 * Hides the box that appears oh hover with the details of the slice
	 * @memberof UmbDonutChartElement
	 */
	@property({ type: Boolean })
	hideDetailBox = false;

	@queryAssignedElements({ selector: 'umb-donut-slice' })
	private _slices!: UmbDonutSliceElement[];

	@query('#container')
	private _container!: HTMLDivElement;

	@query('#details-box')
	private _detailsBox!: HTMLDivElement;

	@state()
	private circles: CircleWithCommands[] = [];

	@state()
	private viewBox = 100;

	@state()
	private _posY = 0;

	@state()
	private _posX = 0;

	@state()
	private _detailName = '';

	@state()
	private _detailAmount = 0;

	@state()
	private _detailColor = 'black';

	@state()
	private _totalAmount = 0;

	@state()
	private _detailKind = '';

	#containerBounds: DOMRect | undefined;

	override firstUpdated() {
		this.#containerBounds = this._container.getBoundingClientRect();
	}

	protected override willUpdate(_changedProperties: Map<PropertyKey, unknown>): void {
		if (_changedProperties.has('radius') || _changedProperties.has('borderSize') || _changedProperties.has('svgSize')) {
			if (this.borderSize > this.radius) {
				throw new Error('Border size cannot be bigger than radius');
			}

			this.#printCircles();
		}
	}

	#calculatePercentage(partialValue: number) {
		if (this._totalAmount === 0) return 0;
		const percent = Math.round((100 * partialValue) / this._totalAmount);
		return clamp(percent, 0, 99);
	}

	#printCircles(event: Event | null = null) {
		this._totalAmount = this._slices.reduce((acc, slice) => acc + slice.amount, 0);
		event?.stopPropagation();
		this.circles = this.#addCommands(
			this._slices.map((slice) => {
				return {
					percent: this.#calculatePercentage(slice.amount),
					number: slice.amount,
					color: slice.color,
					name: slice.name,
					kind: slice.kind,
				};
			}),
		);
	}

	#addCommands(Circles: Circle[]): CircleWithCommands[] {
		let previousPercent = 0;
		return Circles.map((slice) => {
			const sliceWithCommands: CircleWithCommands = {
				...slice,
				commands: this.#getSliceCommands(slice, this.radius, this.svgSize, this.borderSize),
				offset: previousPercent * 3.6 * -1,
			};
			previousPercent += slice.percent;
			return sliceWithCommands;
		});
	}

	#getSliceCommands(Circle: Circle, radius: number, svgSize: number, borderSize: number): string {
		const degrees = UmbDonutChartElement.percentToDegrees(Circle.percent);
		const longPathFlag = degrees > 180 ? 1 : 0;
		const innerRadius = radius - borderSize;

		const commands: string[] = [];
		commands.push(`M ${svgSize / 2 + radius} ${svgSize / 2}`);
		commands.push(`A ${radius} ${radius} 0 ${longPathFlag} 0 ${this.#getCoordFromDegrees(degrees, radius, svgSize)}`);
		commands.push(`L ${this.#getCoordFromDegrees(degrees, innerRadius, svgSize)}`);
		commands.push(`A ${innerRadius} ${innerRadius} 0 ${longPathFlag} 1 ${svgSize / 2 + innerRadius} ${svgSize / 2}`);
		return commands.join(' ');
	}

	#getCoordFromDegrees(angle: number, radius: number, svgSize: number): string {
		const x = Math.cos((angle * Math.PI) / 180);
		const y = Math.sin((angle * Math.PI) / 180);
		const coordX = x * radius + svgSize / 2;
		const coordY = y * -radius + svgSize / 2;
		return [coordX, coordY].join(' ');
	}

	#calculateDetailsBoxPosition = (event: MouseEvent) => {
		const x = this.#containerBounds ? event.clientX - this.#containerBounds?.left : 0;
		const y = this.#containerBounds ? event.clientY - this.#containerBounds?.top : 0;
		this._posX = x - 10;
		this._posY = y - 70;
	};

	#setDetailsBoxData(event: MouseEvent) {
		const target = event.target as SVGPathElement;
		const index = target.dataset.index as unknown as number;
		const circle = this.circles[index];
		this._detailName = circle.name;
		this._detailAmount = circle.number;
		this._detailColor = circle.color;
		this._detailKind = circle.kind;
	}

	#showDetailsBox(event: MouseEvent) {
		if (this.hideDetailBox) return;
		this.#setDetailsBoxData(event);
		this._detailsBox.classList.add('show');
	}

	#hideDetailsBox() {
		if (this.hideDetailBox) return;
		this._detailsBox.classList.remove('show');
	}

	#renderCircles() {
		return svg`
				<filter id="erode" x="-20%" y="-20%" width="140%" height="140%" filterUnits="objectBoundingBox" primitiveUnits="userSpaceOnUse" color-interpolation-filters="linearRGB">
					<feMorphology operator="erode" radius="0.5 0.5" x="0%" y="0%" width="100%" height="100%" in="SourceGraphic" result="morphology"/>
				</filter>
				<filter id="filter" x="-20%" y="-20%" width="140%" height="140%" filterUnits="objectBoundingBox" primitiveUnits="userSpaceOnUse" color-interpolation-filters="linearRGB">
					<feColorMatrix
						type="matrix"
						values="1.8 0 0 0 0
						0 1.8 0 0 0
						0 0 1.8 0 0
						0 0 0 500 -20" x="0%" y="0%" width="100%" height="100%" in="merge1" result="colormatrix2"/>
					<feMorphology operator="erode" radius="0.5 0.5" x="0%" y="0%" width="100%" height="100%" in="colormatrix2" result="morphology2"/>
					<feFlood flood-color="#ffffff" flood-opacity="0.3" x="0%" y="0%" width="100%" height="100%" result="flood3"/>
					<feComposite in="flood3" in2="SourceAlpha" operator="in" x="0%" y="0%" width="100%" height="100%" result="composite3"/>
					<feMorphology operator="erode" radius="1 1" x="0%" y="0%" width="100%" height="100%" in="composite3" result="morphology1"/>
					<feMerge x="0%" y="0%" width="100%" height="100%" result="merge1">
						<feMergeNode in="morphology2"/>
						<feMergeNode in="morphology1"/>
					</feMerge>
					<feDropShadow stdDeviation="1 1" in="merge1" dx="0" dy="0" flood-color="#000" flood-opacity="0.8" x="0%" y="0%" width="100%" height="100%" result="dropShadow1"/>
				</filter>
				<desc>${this.description}</desc>
					${this.circles.map(
						(circle, i) => svg`
								<path
								class="circle"

								data-index="${i}"
									fill="${circle.color}"
									role="listitem"
									d="${circle.commands}"
									transform="rotate(${circle.offset} ${this.viewBox / 2} ${this.viewBox / 2})">
								</path>
								<path
								data-index="${i}"
								@mouseenter=${this.#showDetailsBox}
								@mouseleave=${this.#hideDetailsBox}
									class="highlight"
									fill="${circle.color}"
									role="listitem"
									d="${circle.commands}"
									transform="rotate(${circle.offset} ${this.viewBox / 2} ${this.viewBox / 2})">
								</path>`,
					)}

        `;
	}

	override render() {
		return html` <div id="container" @mousemove=${this.#calculateDetailsBoxPosition}>
				<svg viewBox="0 0 ${this.viewBox} ${this.viewBox}" role="list">${this.#renderCircles()}</svg>
				<div
					id="details-box"
					style="--pos-y: ${this._posY}px; --pos-x: ${this._posX}px; --umb-donut-detail-color: ${this._detailColor}">
					<div id="details-title"><uui-icon name="icon-record"></uui-icon>${this._detailName}</div>
					<span>${this._detailAmount} ${this._detailKind}</span>
				</div>
			</div>
			<slot @slotchange=${this.#printCircles} @slice-update=${this.#printCircles}></slot>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			path {
				pointer-events: visibleFill;
			}
			.circle {
				filter: url(#erode);
			}

			.highlight {
				transition: opacity 200ms linear;
				filter: url(#filter);
				opacity: 0;
			}

			.highlight:hover {
				opacity: 0.5;
			}

			#container {
				position: relative;
				width: 200px;
			}

			#details-box {
				background: #ffffffe6;
				border: 1px solid var(--uui-color-border-standalone);
				border-radius: var(--uui-border-radius);
				box-sizing: border-box;
				top: 0;
				left: 0;
				position: absolute;
				opacity: 0;
				padding: 0.5em;
				line-height: 1.5;
				font-size: var(--uui-type-small-size);
				box-shadow: var(--uui-shadow-depth-1);
				transform: translate3d(var(--pos-x), var(--pos-y), 0);
				transition: transform 0.2s cubic-bezier(0.02, 1.23, 0.79, 1.08);
				transition: opacity 150ms linear;
			}

			#details-box.show {
				opacity: 1;
			}

			#details-box uui-icon {
				/* optically correct alignment */
				color: var(--umb-donut-detail-color);
				margin-right: 0.2em;
			}

			#details-title {
				font-weight: bold;
				display: flex;
				align-items: center;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-donut-chart': UmbDonutChartElement;
	}
}
