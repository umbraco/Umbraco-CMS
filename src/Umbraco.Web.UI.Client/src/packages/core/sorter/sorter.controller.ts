import { isWithinRect } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

const autoScrollSensitivity = 50;
const autoScrollSpeed = 16;

/**
 *
 * @param el
 * @param includeSelf
 */
function getParentScrollElement(el: Element, includeSelf: boolean) {
	if (!el || !el.getBoundingClientRect) return null;

	let elem = el;
	let gotSelf = false;

	while (elem) {
		// we don't need to get elem css if it isn't even overflowing in the first place (performance)
		if (elem.clientWidth < elem.scrollWidth || elem.clientHeight < elem.scrollHeight) {
			const elemCSS = getComputedStyle(elem);

			if (
				(elem.clientHeight < elem.scrollHeight && (elemCSS.overflowY == 'auto' || elemCSS.overflowY == 'scroll')) ||
				(elem.clientWidth < elem.scrollWidth && (elemCSS.overflowX == 'auto' || elemCSS.overflowX == 'scroll'))
			) {
				if (!elem.getBoundingClientRect || elem === document.body) return null;
				if (gotSelf || includeSelf) return elem;
				gotSelf = true;
			}
		}

		if (elem.parentNode === document) {
			return null;
		} else if (elem.parentNode instanceof ShadowRoot) {
			elem = elem.parentNode.host;
		} else {
			elem = elem.parentNode as Element;
		}
	}

	return null;
}

/**
 *
 * @param element
 * @param ignorerSelectors
 */
function setupIgnorerElements(element: HTMLElement, ignorerSelectors: string) {
	ignorerSelectors.split(',').forEach(function (criteria) {
		element.querySelectorAll(criteria.trim()).forEach(setupPreventEvent);
	});
}
/**
 *
 * @param element
 * @param ignorerSelectors
 */
function destroyIgnorerElements(element: HTMLElement, ignorerSelectors: string) {
	ignorerSelectors.split(',').forEach(function (criteria: string) {
		element.querySelectorAll(criteria.trim()).forEach(destroyPreventEvent);
	});
}
/**
 *
 * @param element
 */
function setupPreventEvent(element: Element) {
	(element as HTMLElement).draggable = false;
	//(element as HTMLElement).setAttribute('draggable', 'false');
}
/**
 *
 * @param element
 */
function destroyPreventEvent(element: Element) {
	(element as HTMLElement).draggable = false;
	//element.removeAttribute('draggable');
}

export type resolvePlacementArgs<T, ElementType extends HTMLElement> = {
	containerElement: Element;
	containerRect: DOMRect;
	item: T;
	element: ElementType;
	elementRect: DOMRect;
	relatedElement: ElementType;
	relatedModel: T;
	relatedRect: DOMRect;
	placeholderIsInThisRow: boolean;
	horizontalPlaceAfter: boolean;
	pointerX: number;
	pointerY: number;
};

type UniqueType = string | symbol | number;

/**
 * Internal type, which is adjusted to become the public one.
  @internal */
type INTERNAL_UmbSorterConfig<T, ElementType extends HTMLElement> = {
	/**
	 * Define how to retrive the unique identifier of an element. If this method returns undefined, the move will be cancelled.
	 */
	getUniqueOfElement: (element: ElementType) => UniqueType | null | undefined;
	getUniqueOfModel: (modeEntry: T) => UniqueType | null | undefined;
	/**
	 * Optionally define a unique identifier for each sorter experience, all Sorters that uses the same identifier to connect with other sorters.
	 */
	identifier: UniqueType;
	/**
	 * A query selector for the item element.
	 */
	itemSelector: string;
	disabledItemSelector?: string;
	/**
	 * A selector for the container element, if not defined the host element will be used as container.
	 */
	containerSelector: string;
	/**
	 * A selector for elements to ignore, elements that should not be draggable when within an draggable item, This defaults to links, images & iframes.
	 */
	ignorerSelector: string;
	/**
	 * An class to set on the placeholder element.
	 */
	placeholderClass?: string;
	/**
	 * An attribute to set on the placeholder element.
	 */
	placeholderAttr?: string;
	/**
	 * The selector to find the draggable element within the item.
	 */
	draggableSelector?: string;

	//boundarySelector?: string;
	dataTransferResolver?: (dataTransfer: DataTransfer | null, currentItem: T) => void;
	onStart?: (argument: { item: T; element: ElementType }) => void;
	/**
	 * This callback is executed every time where is a change to this model, this could be a move, insert or remove.
	 * But notice its not called if a more specific callback is provided, such would be the performItemMove, performItemInsert or performItemRemove or performItemRemove.
	 */
	onChange?: (argument: { item: T; model: Array<T> }) => void;
	/**
	 * This callback is executed when an item is moved from another container to this container.
	 */
	onContainerChange?: (argument: { item: T; model: Array<T>; from: UmbSorterController<T, ElementType> }) => void;
	onEnd?: (argument: { item: T; element: ElementType }) => void;
	itemHasNestedContainersResolver?: (element: HTMLElement) => boolean;
	/**
	 * Callback when a item move is disallowed.
	 * This should make a visual indication for the user to understand that the move is not allowed.
	 */
	onDisallowed?: (argument: { item: T; element: ElementType }) => void;
	/**
	 * Callback when a item move is allowed.
	 * This should remove any visual indication of the disallowing, reverting the work of the onDisallowed callback.
	 */
	onAllowed?: (argument: { item: T; element: ElementType }) => void;
	/**
	 * Callback when user tries to move an item from another Sorter to this Sorter, return true or false to allow or disallow the move.
	 */
	onRequestMove?: (argument: { item: T }) => boolean;
	/**
	 * This callback is executed when an item is hovered within this container.
	 * The callback should return true if the item should be placed after the hovered item, or false if it should be placed before the hovered item.
	 * In this way the callback can control the placement of the item.
	 * If it returns null the placement will be prevented.
	 * @example
	 * This is equivalent to the default behavior:
	 * ```ts
	 * resolvePlacement: (argument) => {
	 * 	if(argument.pointerY > argument.relatedRect.top + argument.relatedRect.height * 0.5) {
	 * 		return true; // Place after
	 * 	} else {
	 * 		return false; // Place before
	 * 	}
	 * }
	 */
	resolvePlacement?: (argument: resolvePlacementArgs<T, ElementType>) => boolean | null;
	/**
	 * This callback is executed when an item is moved within this container.
	 */
	performItemMove?: (argument: { item: T; newIndex: number; oldIndex: number }) => Promise<boolean> | boolean;
	/**
	 * This callback is executed when an item should be inserted into this container.
	 */
	performItemInsert?: (argument: { item: T; newIndex: number }) => Promise<boolean> | boolean;
	/**
	 * This callback is executed when an item should be removed from this container.
	 */
	performItemRemove?: (argument: { item: T }) => Promise<boolean> | boolean;
};

// External type with some properties optional, as they have fallback values:
export type UmbSorterConfig<T, ElementType extends HTMLElement = HTMLElement> = Omit<
	INTERNAL_UmbSorterConfig<T, ElementType>,
	'ignorerSelector' | 'containerSelector' | 'identifier'
> &
	Partial<Pick<INTERNAL_UmbSorterConfig<T, ElementType>, 'ignorerSelector' | 'containerSelector' | 'identifier'>>;

/**
 
 * @class UmbSorterController
 * @implements {UmbControllerInterface}
 * @description This controller can make user able to sort items.
 * @example
 *
 * This example shows how to setup a sorter controller with no special needs.
 * Assuming your declaring this on a Umbraco Element(UmbControllerHostElement):
 *
 * ```ts
 * const sorter = new UmbSorterController(this, {
 * 	itemSelector: '.item',
 * 	containerSelector: '.container',
 * 	getUniqueOfElement: (element) => element.dataset.id,
 * 	getUniqueOfModel: (model) => model.id
 * });
 * ```
 */
export class UmbSorterController<T, ElementType extends HTMLElement = HTMLElement> extends UmbControllerBase {
	//
	// The sorter who last indicated that it was okay or not okay to drop here:
	static lastIndicationSorter?: UmbSorterController<unknown>;

	// A sorter that is requested to become the next sorter:
	static originalSorter?: UmbSorterController<unknown>;
	static originalIndex?: number;

	// A sorter that is requested to become the next sorter:
	static dropSorter?: UmbSorterController<unknown>;

	// The sorter of which the element is located within:
	static activeSorter?: UmbSorterController<unknown>;

	// Information about the current dragged item/element:
	static activeIndex?: number;
	static activeItem?: any;
	static activeElement?: HTMLElement;
	static activeDragElement?: Element;

	#host;
	#isConnected = false;
	#config: INTERNAL_UmbSorterConfig<T, ElementType>;
	#observer;

	#model: Array<T> = [];
	#rqaId?: number;

	#containerElement!: HTMLElement;
	#useContainerShadowRoot?: boolean;

	#scrollElement?: Element | null;

	#enabled = true;

	#dragX = 0;
	#dragY = 0;

	#items = Array<ElementType>();

	public get identifier() {
		return this.#config.identifier;
	}
	public set identifier(identifier: UniqueType) {
		this.#config.identifier = identifier;
	}
	#isActiveIdentifier() {
		return UmbSorterController.activeSorter?.identifier === this.identifier;
	}

	constructor(host: UmbControllerHostElement, config: UmbSorterConfig<T, ElementType>) {
		super(host);
		this.#host = host;

		// Set defaults:
		config.identifier ??= Symbol();
		config.ignorerSelector ??= 'a, img, iframe';
		if (!config.placeholderClass && !config.placeholderAttr) {
			config.placeholderAttr = 'drag-placeholder';
		}

		this.#config = config as INTERNAL_UmbSorterConfig<T, ElementType>;
		host.addUmbController(this);

		this.#observer = new MutationObserver((mutations) => {
			mutations.forEach((mutation) => {
				mutation.addedNodes.forEach((addedNode) => {
					if ((addedNode as HTMLElement).matches && (addedNode as HTMLElement).matches(this.#config.itemSelector)) {
						this.setupItem(addedNode as ElementType);
					}
				});
				mutation.removedNodes.forEach((removedNode) => {
					if ((removedNode as HTMLElement).matches && (removedNode as HTMLElement).matches(this.#config.itemSelector)) {
						this.destroyItem(removedNode as ElementType);
					}
				});
			});
		});
	}

	/**
	 * Enables the sorter, this will allow sorting to happen.
	 * @returns {*}  {void}
	 * @memberof UmbSorterController
	 */
	enable(): void {
		if (this.#enabled) return;
		this.#enabled = true;
		if (this.#isConnected) {
			this.#initialize();
		}
	}

	/**
	 * Disables the sorter, this will prevent any sorting to happen.
	 * @returns {*}  {void}
	 * @memberof UmbSorterController
	 */
	disable(): void {
		if (!this.#enabled) return;
		this.#enabled = false;
		if (this.#isConnected) {
			this.#uninitialize();
		}
	}

	setModel(model: Array<T>): void {
		if (this.#model) {
			// TODO: Some updates might need to be done, as the model is about to change? Do make the changes after setting the model?.. [NL]
			this.#model = model;
		}
	}

	/**
	 * Returns the model of the sorter.
	 * @returns {Array<T>}
	 * @memberof UmbSorterController
	 */
	getModel(): Array<T> {
		return this.#model;
	}

	hasItem(unique: UniqueType) {
		return this.#model.find((x) => this.#config.getUniqueOfModel(x) === unique) !== undefined;
	}

	getItem(unique: UniqueType) {
		return this.#model.find((x) => this.#config.getUniqueOfModel(x) === unique);
	}

	override hostConnected() {
		this.#isConnected = true;
		if (this.#enabled) {
			requestAnimationFrame(this.#initialize);
		}
	}

	override hostDisconnected() {
		this.#isConnected = false;
		if (this.#enabled) {
			this.#uninitialize();
		}
	}

	#initialize = () => {
		const containerEl =
			(this.#config.containerSelector
				? this.#host.shadowRoot!.querySelector(this.#config.containerSelector)
				: this.#host) ?? this.#host;

		this.#containerElement = containerEl as HTMLElement;
		this.#useContainerShadowRoot = this.#containerElement === this.#host;

		// Only look at the shadowRoot if the containerElement is host.
		const containerElement = this.#useContainerShadowRoot
			? (this.#containerElement.shadowRoot ?? this.#containerElement)
			: this.#containerElement;
		containerElement.addEventListener('dragover', this._itemDraggedOver as unknown as EventListener);

		// TODO: Do we need to handle dragleave?

		this.#observer.disconnect();

		containerElement.querySelectorAll(this.#config.itemSelector).forEach((child) => {
			if (child.matches && child.matches(this.#config.itemSelector)) {
				this.setupItem(child as ElementType);
			}
		});
		this.#observer.observe(containerElement, {
			childList: true,
			subtree: false,
		});
	};

	#uninitialize() {
		// TODO: Is there more clean up to do??
		this.#observer.disconnect();
		if (this.#containerElement) {
			// Only look at the shadowRoot if the containerElement is host.
			const containerElement = this.#useContainerShadowRoot
				? (this.#containerElement.shadowRoot ?? this.#containerElement)
				: this.#containerElement;

			containerElement.removeEventListener('dragover', this._itemDraggedOver as unknown as EventListener);
			(this.#containerElement as unknown) = undefined;
		}

		this.#items.forEach((item) => this.destroyItem(item));
	}

	_itemDraggedOver = (e: DragEvent) => {
		//if(UmbSorterController.activeSorter === this) return;
		const dropSorter = UmbSorterController.dropSorter as unknown as UmbSorterController<T, ElementType>;
		if (!dropSorter || dropSorter.identifier !== this.identifier) return;

		if (dropSorter === this) {
			e.preventDefault();
			if (e.dataTransfer) {
				e.dataTransfer.dropEffect = 'move';
			}

			// Do nothing as we are the active sorter.
			this.#handleDragMove(e);

			// Maybe we need to stop the event in this case.

			// Do not bubble up to parent sorters:
			e.stopPropagation();

			return;
		} else {
			// TODO: Check if dropping here is okay..

			// If so lets set the approaching sorter:
			UmbSorterController.dropSorter = this as unknown as UmbSorterController<unknown>;

			// Do not bubble up to parent sorters:
			e.stopPropagation();
		}
	};

	#getDraggableElement(element: HTMLElement) {
		if (this.#config.draggableSelector) {
			// Concept for enabling getting element within ShadowRoot: (But it might need to be configurable, so its still possible to get light dom element(slotted), despite the host is a web-component with shadow-dom.) [NL]
			//const queryFromEl = element.shadowRoot ?? element;
			return (element.querySelector(this.#config.draggableSelector) as HTMLElement | undefined) ?? element;
		}
		return element;
	}

	setupItem(element: ElementType) {
		if (this.#config.ignorerSelector) {
			setupIgnorerElements(element, this.#config.ignorerSelector);
		}

		if (!this.#config.disabledItemSelector || !element.matches(this.#config.disabledItemSelector)) {
			// Idea: to make sure on does not get initialized twice: if ((element as HTMLElement).draggable === true) return;
			const draggableElement = this.#getDraggableElement(element);
			(draggableElement as HTMLElement).draggable = true;
			draggableElement.addEventListener('dragstart', this.#handleDragStart);
			draggableElement.addEventListener('dragend', this.#handleDragEnd);
		}

		// If we have a currentItem and the element matches, we should set the currentElement to this element.
		if (UmbSorterController.activeItem && this.#isActiveIdentifier()) {
			const elUnique = this.#config.getUniqueOfElement(element);
			const modelUnique = this.#config.getUniqueOfModel(UmbSorterController.activeItem);
			if (elUnique === modelUnique && elUnique !== undefined) {
				if (UmbSorterController.activeElement !== element) {
					this.#setCurrentElement(element);
				}
			}
		}

		this.#items.push(element);
		this.#items = Array.from(new Set(this.#items));
	}

	destroyItem(element: HTMLElement) {
		if (this.#config.ignorerSelector) {
			destroyIgnorerElements(element, this.#config.ignorerSelector);
		}

		const draggableElement = this.#getDraggableElement(element);
		draggableElement.removeEventListener('dragstart', this.#handleDragStart);
		// We are not ready to remove the dragend or drop, as this is might be the active one just moving container:
		//draggableElement.removeEventListener('dragend', this.#handleDragEnd);

		(draggableElement as HTMLElement).draggable = false;

		this.#items = this.#items.filter((x) => x !== element);
	}

	#setupPlaceholderStyle() {
		if (this.#config.placeholderClass) {
			UmbSorterController.activeElement?.classList.add(this.#config.placeholderClass);
		}
		if (this.#config.placeholderAttr) {
			UmbSorterController.activeElement?.setAttribute(this.#config.placeholderAttr, '');
		}
	}
	#removePlaceholderStyle() {
		if (this.#config.placeholderClass) {
			UmbSorterController.activeElement?.classList.remove(this.#config.placeholderClass);
		}
		if (this.#config.placeholderAttr) {
			UmbSorterController.activeElement?.removeAttribute(this.#config.placeholderAttr);
		}
	}

	#setCurrentElement(element: ElementType) {
		UmbSorterController.activeElement = element;

		UmbSorterController.activeDragElement = this.#getDraggableElement(element);

		if (!UmbSorterController.activeDragElement) {
			throw new Error(
				'Could not find drag element, query was made with the `draggableSelector` of "' +
					this.#config.draggableSelector +
					'"',
			);
			return;
		}

		this.#setupPlaceholderStyle();
	}

	#handleDragStart = (event: DragEvent) => {
		const element = (event.target as HTMLElement).closest(this.#config.itemSelector);
		if (!element) return;

		if (UmbSorterController.activeElement && UmbSorterController.activeElement !== element) {
			// TODO: Remove this console log at one point.
			console.error('drag start ws cancelled due to another drag was still active');
			this.#handleDragEnd();
		}

		event.stopPropagation();
		if (event.dataTransfer) {
			event.dataTransfer.effectAllowed = 'all'; // copyMove when we enhance the drag with clipboard data.// defaults to 'all'
		}

		if (!this.#scrollElement) {
			this.#scrollElement = getParentScrollElement(this.#containerElement, true);
		}

		this.#setCurrentElement(element as ElementType);
		UmbSorterController.activeItem = this.getItemOfElement(UmbSorterController.activeElement! as ElementType);

		UmbSorterController.originalSorter = this as unknown as UmbSorterController<unknown>;
		// Notice, it is acceptable here to get index via object reference, but only cause there has been no change at this stage, otherwise we cannot trust the object instance is represented in the model — it could have mutated or been cloned [NL]
		UmbSorterController.originalIndex = this.#model.indexOf(UmbSorterController.activeItem);

		if (!UmbSorterController.activeItem) {
			console.error('Could not find item related to this element.', UmbSorterController.activeElement);
			return;
		}

		// Get the current index of the item:
		UmbSorterController.activeIndex = UmbSorterController.originalIndex;

		UmbSorterController.activeElement!.style.transform = 'translateZ(0)'; // Solves problem with FireFox and ShadowDom in the drag-image.

		if (this.#config.dataTransferResolver) {
			this.#config.dataTransferResolver(event.dataTransfer, UmbSorterController.activeItem as T);
		}

		if (this.#config.onStart) {
			this.#config.onStart({
				item: UmbSorterController.activeItem,
				element: UmbSorterController.activeElement! as ElementType,
			});
		}

		// Assuming we can only drag one thing at the time.
		UmbSorterController.activeSorter = this as unknown as UmbSorterController<unknown>;
		UmbSorterController.dropSorter = this as unknown as UmbSorterController<unknown>;

		// We must wait one frame before changing the look of the block.
		this.#rqaId = requestAnimationFrame(() => {
			// It should be okay to use the same rqaId, as the move does not, or is okay not, to happen on first frame/drag-move.
			this.#rqaId = undefined;
			if (UmbSorterController.activeElement) {
				UmbSorterController.activeElement.style.transform = '';
			}
		});

		return true;
	};

	#handleDragEnd = async (event?: DragEvent) => {
		if (!UmbSorterController.activeElement || !UmbSorterController.activeItem) {
			return;
		}

		// If browser thinks this was a cancelled move, we should revert the move. (based on dropEffect === 'none') [NL]
		// But notice, this also count when releasing the mouse outside the sorters element, this i'm not sure if I agree on, would be ideal only to revert if ESC was pressed. [NL]
		if (UmbSorterController.originalSorter && event?.dataTransfer != null && event.dataTransfer.dropEffect === 'none') {
			// Revert move, to start position.
			UmbSorterController.originalSorter.moveItemInModel(
				UmbSorterController.originalIndex ?? 0,
				UmbSorterController.activeSorter!,
			);
		}

		UmbSorterController.activeElement.style.transform = '';
		this.#removePlaceholderStyle();

		this.#stopAutoScroll();
		this.removeAllowIndication();

		if (this.#config.onEnd) {
			this.#config.onEnd({
				item: UmbSorterController.activeItem,
				element: UmbSorterController.activeElement as ElementType,
			});
		}

		if (this.#rqaId) {
			cancelAnimationFrame(this.#rqaId);
			this.#rqaId = undefined;
		}

		UmbSorterController.activeItem = undefined;
		UmbSorterController.activeElement = undefined;
		UmbSorterController.activeDragElement = undefined;
		UmbSorterController.activeSorter = undefined;
		UmbSorterController.dropSorter = undefined;
		UmbSorterController.originalIndex = undefined;
		UmbSorterController.originalSorter = undefined;
		this.#dragX = 0;
		this.#dragY = 0;
	};

	#handleDragMove(event: DragEvent) {
		if (!UmbSorterController.activeElement) {
			return;
		}

		const clientX = (event as unknown as TouchEvent).touches
			? (event as unknown as TouchEvent).touches[0].clientX
			: event.clientX;
		const clientY = (event as unknown as TouchEvent).touches
			? (event as unknown as TouchEvent).touches[0].clientY
			: event.clientY;
		if (clientX !== 0 && clientY !== 0) {
			if (this.#dragX === clientX && this.#dragY === clientY) {
				return;
			}
			this.#dragX = clientX;
			this.#dragY = clientY;

			this.handleAutoScroll(this.#dragX, this.#dragY);

			const activeDragRect = UmbSorterController.activeDragElement!.getBoundingClientRect();
			const insideCurrentRect = isWithinRect(this.#dragX, this.#dragY, activeDragRect);
			if (!insideCurrentRect) {
				if (this.#rqaId === undefined) {
					this.#rqaId = requestAnimationFrame(this.#updateDragMove);
				}
			}
		}
	}

	#updateDragMove = () => {
		this.#rqaId = undefined;
		if (!UmbSorterController.activeElement || !UmbSorterController.activeItem) {
			return;
		}

		// Maybe no need to check this twice, like we do it before the RAF an inside it, I think its fine to choose one of them.
		const currentElementRect = UmbSorterController.activeElement.getBoundingClientRect();
		const insideCurrentRect = isWithinRect(this.#dragX, this.#dragY, currentElementRect);
		if (insideCurrentRect) {
			return;
		}

		const containerElement = this.#useContainerShadowRoot
			? (this.#containerElement.shadowRoot ?? this.#containerElement)
			: this.#containerElement;

		// We want to retrieve the children of the container, every time to ensure we got the right order and index
		const orderedContainerElements = Array.from(containerElement.querySelectorAll(this.#config.itemSelector));

		const currentContainerRect = this.#containerElement.getBoundingClientRect();

		// gather elements on the same row.
		const elementsInSameRow = [];
		let placeholderIsInThisRow = false;
		for (const el of orderedContainerElements) {
			const elRect = el.getBoundingClientRect();
			// gather elements on the same row.
			if (this.#dragY >= elRect.top && this.#dragY <= elRect.bottom) {
				const dragElement = this.#getDraggableElement(el as unknown as HTMLElement);
				if (dragElement) {
					const dragElementRect = dragElement.getBoundingClientRect();
					if (el !== UmbSorterController.activeElement) {
						elementsInSameRow.push({ el: el, dragRect: dragElementRect });
					} else {
						placeholderIsInThisRow = true;
					}
				}
			}
		}

		let lastDistance = Infinity;
		let foundEl: HTMLElement | undefined = undefined;
		let foundElDragRect!: DOMRect;
		let placeAfter = false;
		elementsInSameRow.forEach((sameRow) => {
			const centerX = sameRow.dragRect.left + sameRow.dragRect.width * 0.5;
			const distance = Math.abs(this.#dragX - centerX);
			if (distance < lastDistance) {
				foundEl = sameRow.el as HTMLElement;
				foundElDragRect = sameRow.dragRect;
				lastDistance = distance;
				placeAfter = this.#dragX > centerX;
			}
		});

		if (foundEl) {
			// If we are on top or closest to our self, we should not do anything.
			if (foundEl === UmbSorterController.activeElement) {
				return;
			}

			const foundModel = this.getItemOfElement(foundEl);
			if (!foundModel) {
				throw new Error('Could not find model of found element');
			}

			// Indication if drop is good:
			if (this.updateAllowIndication(UmbSorterController.activeItem) === false) {
				return;
			}

			const verticalDirection: boolean | null = this.#config.resolvePlacement
				? this.#config.resolvePlacement({
						containerElement: this.#containerElement,
						containerRect: currentContainerRect,
						item: UmbSorterController.activeItem,
						element: UmbSorterController.activeElement as ElementType,
						elementRect: currentElementRect,
						relatedElement: foundEl,
						relatedModel: foundModel,
						relatedRect: foundElDragRect,
						placeholderIsInThisRow: placeholderIsInThisRow,
						horizontalPlaceAfter: placeAfter,
						pointerX: this.#dragX,
						pointerY: this.#dragY,
					})
				: true;

			if (verticalDirection === null) {
				// The resolvePlacement has chosen to back out of this move.
				return;
			}

			if (verticalDirection) {
				placeAfter = this.#dragY > foundElDragRect.top + foundElDragRect.height * 0.5;
			}

			if (verticalDirection) {
				let el;
				if (placeAfter === false) {
					let lastLeft = foundElDragRect.left;
					elementsInSameRow.findIndex((x) => {
						if (x.dragRect.left < lastLeft) {
							lastLeft = x.dragRect.left;
							el = x.el;
						}
					});
				} else {
					let lastRight = foundElDragRect.right;
					elementsInSameRow.findIndex((x) => {
						if (x.dragRect.right > lastRight) {
							lastRight = x.dragRect.right;
							el = x.el;
						}
					});
				}
				if (el) {
					foundEl = el;
				}
			}

			const foundElIndex = orderedContainerElements.indexOf(foundEl);
			const newIndex = placeAfter ? foundElIndex + 1 : foundElIndex;
			this.#moveElementTo(newIndex);

			return;
		}
		// We skipped the above part cause we are above or below container, or within an empty container:

		// Indication if drop is good:
		if (this.updateAllowIndication(UmbSorterController.activeItem) === false) {
			return;
		}

		if (this.#model.length === 0) {
			// Here is no items, so we should just move into the top of the container.
			this.#moveElementTo(0);
		} else if (this.#dragY < currentContainerRect.top) {
			this.#moveElementTo(0);
		} else if (this.#dragY > currentContainerRect.bottom) {
			this.#moveElementTo(-1);
		}
	};

	//
	async #moveElementTo(newIndex: number) {
		if (!UmbSorterController.activeElement || !UmbSorterController.activeSorter) {
			return;
		}

		const requestingSorter = UmbSorterController.dropSorter;
		if (!requestingSorter) {
			throw new Error('Could not find requestingSorter');
		}

		// If same container and same index, do nothing:
		if (requestingSorter === UmbSorterController.activeSorter && UmbSorterController.activeIndex === newIndex) return;

		await requestingSorter.moveItemInModel(newIndex, UmbSorterController.activeSorter);
	}

	/** Management methods: */

	public getItemOfElement(element: ElementType) {
		if (!element) {
			throw new Error('Element was not defined');
		}
		const elementUnique = this.#config.getUniqueOfElement(element);
		if (elementUnique === undefined) {
			console.error('Could not find unique of element', element);
			return undefined;
		}
		return this.#model.find((entry: T) => elementUnique === this.#config.getUniqueOfModel(entry));
	}

	public async removeItem(item: T) {
		if (!item) {
			return false;
		}

		if (this.#config.performItemRemove) {
			return (await this.#config.performItemRemove({ item })) ?? false;
		} else {
			const oldIndex = this.#model.indexOf(item);
			if (oldIndex !== -1) {
				const newModel = [...this.#model];
				newModel.splice(oldIndex, 1);
				this.#model = newModel;
				this.#config.onChange?.({ model: newModel, item });
				return true;
			}
		}
		return false;
	}
	/*
	public async insertItem(item: T, newIndex: number = 0) {
		if (!item) {
			return false;
		}

		if (this.#config.performItemInsert) {
			const result = await this.#config.performItemInsert({ item, newIndex });
			if (result === false) {
				return false;
			}
		} else {
			const newModel = [...this.#model];
			newModel.splice(newIndex, 0, item);
			this.#model = newModel;
			this.#config.onChange?.({ model: newModel, item });
		}
		return false;
	}
	*/

	public hasOtherItemsThan(item: T) {
		return this.#model.filter((x) => x !== item).length > 0;
	}

	// TODO: Could get item via attr.
	public async moveItemInModel(newIndex: number, fromCtrl: UmbSorterController<T, ElementType>) {
		if (!UmbSorterController.activeItem) {
			console.error('There is no active item to move');
			return false;
		}
		const itemUnique = this.#config.getUniqueOfModel(UmbSorterController.activeItem);
		if (!itemUnique) {
			console.error('Failed to retrieve active item unique');
			return false;
		}
		// We use the getItem method to find the current item/object of this entry, as we cannot trust the object instance(activeItem) to be the same as in the model. [NL]
		// So notice, item in this method is the real modal entry reference, where in many other cases we use the activeItem which might not be up to date with the real entry of the model. [NL]
		const item = fromCtrl.getItem(itemUnique);
		if (!item) {
			console.error('Could not find item of model to move', itemUnique, this.#model);
			return false;
		}
		if (this.notifyRequestDrop({ item }) === false) {
			return false;
		}

		const localMove = fromCtrl === (this as any);

		if (localMove) {
			// Local move:

			const oldIndex = this.#model.indexOf(item);
			if (oldIndex === -1) {
				console.error('Could not find item in model when performing internal move', this.getHostElement(), this.#model);
				return false;
			}

			if (this.#config.performItemMove) {
				const result = await this.#config.performItemMove({ item, newIndex, oldIndex });
				if (result === false) {
					return false;
				}
			} else {
				const newModel = [...this.#model];
				newModel.splice(oldIndex, 1);
				if (oldIndex <= newIndex) {
					newIndex--;
				}
				newModel.splice(newIndex, 0, item);
				this.#model = newModel;
				this.#config.onChange?.({ model: newModel, item });
			}

			UmbSorterController.activeIndex = newIndex;
		} else {
			// Not a local move:

			if ((await fromCtrl.removeItem(item)) !== true) {
				console.error('Sync could not remove item when moving to a new container');
				return false;
			}

			if (this.#config.performItemInsert) {
				const result = await this.#config.performItemInsert({ item, newIndex });
				if (result === false) {
					console.error('Sync could not insert after a move a new container');
					return false;
				}
			} else {
				const newModel = [...this.#model];
				newModel.splice(newIndex, 0, item);
				this.#model = newModel;

				this.#config.onContainerChange?.({
					model: newModel,
					item,
					from: fromCtrl as unknown as UmbSorterController<T, ElementType>,
				});
				this.#config.onChange?.({ model: newModel, item });

				// If everything went well, we can set the new activeSorter (and dropSorter) to this, as we are switching container. [NL]
				UmbSorterController.activeSorter = this as unknown as UmbSorterController<unknown>;
				UmbSorterController.dropSorter = this as unknown as UmbSorterController<unknown>;
				UmbSorterController.activeIndex = newIndex;
			}
		}

		return true;
	}

	updateAllowIndication(item: T) {
		// Remove old indication:
		if (UmbSorterController.lastIndicationSorter && UmbSorterController.lastIndicationSorter !== (this as unknown)) {
			UmbSorterController.lastIndicationSorter.notifyAllowed();
		}
		UmbSorterController.lastIndicationSorter = this as unknown as UmbSorterController<unknown>;

		if (this.notifyRequestDrop({ item: item }) === true) {
			this.notifyAllowed();
			return true;
		}

		this.notifyDisallowed(); // This block is not accepted to we will indicate that its not allowed.
		return false;
	}
	removeAllowIndication() {
		// Remove old indication:
		if (UmbSorterController.lastIndicationSorter) {
			UmbSorterController.lastIndicationSorter.notifyAllowed();
		}
		UmbSorterController.lastIndicationSorter = undefined;
	}

	// TODO: Move auto scroll into its own class?
	#autoScrollRAF: number | null = null;
	#autoScrollEl = document.scrollingElement || document.documentElement;
	private autoScrollX = 0;
	private autoScrollY = 0;

	private handleAutoScroll(clientX: number, clientY: number) {
		let scrollRect: DOMRect | null = null;
		if (this.#scrollElement) {
			this.#autoScrollEl = this.#scrollElement;
			scrollRect = this.#autoScrollEl.getBoundingClientRect();
		} else {
			this.#autoScrollEl = document.scrollingElement || document.documentElement;
			scrollRect = {
				top: 0,
				left: 0,
				bottom: window.innerHeight,
				right: window.innerWidth,
				height: window.innerHeight,
				width: window.innerWidth,
			} as DOMRect;
		}

		const scrollWidth = this.#autoScrollEl.scrollWidth;
		const scrollHeight = this.#autoScrollEl.scrollHeight;
		const canScrollX = scrollRect.width < scrollWidth;
		const canScrollY = scrollRect.height < scrollHeight;
		const scrollPosX = this.#autoScrollEl.scrollLeft;
		const scrollPosY = this.#autoScrollEl.scrollTop;

		cancelAnimationFrame(this.#autoScrollRAF!);

		if (canScrollX || canScrollY) {
			this.autoScrollX =
				Math.abs(scrollRect.right - clientX) <= autoScrollSensitivity && scrollPosX + scrollRect.width < scrollWidth
					? 1
					: Math.abs(scrollRect.left - clientX) <= autoScrollSensitivity && !!scrollPosX
						? -1
						: 0;

			this.autoScrollY =
				Math.abs(scrollRect.bottom - clientY) <= autoScrollSensitivity && scrollPosY + scrollRect.height < scrollHeight
					? 1
					: Math.abs(scrollRect.top - clientY) <= autoScrollSensitivity && !!scrollPosY
						? -1
						: 0;

			this.#autoScrollRAF = requestAnimationFrame(this.#performAutoScroll);
		}
	}
	#performAutoScroll = () => {
		this.#autoScrollEl!.scrollLeft += this.autoScrollX * autoScrollSpeed;
		this.#autoScrollEl!.scrollTop += this.autoScrollY * autoScrollSpeed;
		this.#autoScrollRAF = requestAnimationFrame(this.#performAutoScroll);
	};
	#stopAutoScroll() {
		cancelAnimationFrame(this.#autoScrollRAF!);
		this.#autoScrollRAF = null;
	}

	public notifyDisallowed() {
		if (this.#config.onDisallowed) {
			this.#config.onDisallowed({
				item: UmbSorterController.activeItem,
				element: UmbSorterController.activeElement! as ElementType,
			});
		}
	}
	public notifyAllowed() {
		if (this.#config.onAllowed) {
			this.#config.onAllowed({
				item: UmbSorterController.activeItem,
				element: UmbSorterController.activeElement! as ElementType,
			});
		}
	}
	public notifyRequestDrop(data: any) {
		if (this.#config.onRequestMove) {
			return this.#config.onRequestMove(data) || false;
		}
		return true;
	}

	override destroy() {
		super.destroy();

		// Do something when host element is destroyed.
		if (UmbSorterController.activeElement) {
			this.#handleDragEnd();
		}

		UmbSorterController.lastIndicationSorter = undefined;

		// TODO: Clean up items??
		this.#observer.disconnect();

		// For auto scroller:
		this.#scrollElement = null;
	}
}
