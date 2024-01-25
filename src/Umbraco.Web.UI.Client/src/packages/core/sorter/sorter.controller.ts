import type { UmbController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

const autoScrollSensitivity = 50;
const autoScrollSpeed = 16;

function isWithinRect(x: number, y: number, rect: DOMRect, modifier = 0) {
	return x > rect.left - modifier && x < rect.right + modifier && y > rect.top - modifier && y < rect.bottom + modifier;
}

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

function preventDragOver(e: Event) {
	e.preventDefault();
}

function setupIgnorerElements(element: HTMLElement, ignorerSelectors: string) {
	ignorerSelectors.split(',').forEach(function (criteria) {
		element.querySelectorAll(criteria.trim()).forEach(setupPreventEvent);
	});
}
function destroyIgnorerElements(element: HTMLElement, ignorerSelectors: string) {
	ignorerSelectors.split(',').forEach(function (criteria: string) {
		element.querySelectorAll(criteria.trim()).forEach(destroyPreventEvent);
	});
}
function setupPreventEvent(element: Element) {
	(element as HTMLElement).draggable = false;
}
function destroyPreventEvent(element: Element) {
	element.removeAttribute('draggable');
}

type INTERNAL_UmbSorterConfig<T, ElementType extends HTMLElement> = {
	compareElementToModel: (el: ElementType, modelEntry: T) => boolean;
	querySelectModelToElement: (container: HTMLElement, modelEntry: T) => ElementType | null;
	identifier: string;
	itemSelector: string;
	disabledItemSelector?: string;
	containerSelector: string;
	ignorerSelector: string;
	placeholderClass?: string;
	placeholderAttr?: string;
	draggableSelector?: string;
	boundarySelector?: string;
	dataTransferResolver?: (dataTransfer: DataTransfer | null, currentItem: T) => void;
	onStart?: (argument: { item: T; element: ElementType }) => void;
	onChange?: (argument: { item: T; model: Array<T> }) => void;
	onContainerChange?: (argument: { item: T; element: ElementType }) => void;
	onEnd?: (argument: { item: T; element: ElementType }) => void;
	itemHasNestedContainersResolver?: (element: HTMLElement) => boolean;
	onDisallowed?: () => void;
	onAllowed?: () => void;
	onRequestDrop?: (argument: { item: T }) => boolean | void;
	resolveVerticalDirection?: (argument: {
		containerElement: Element;
		containerRect: DOMRect;
		item: T;
		element: ElementType;
		elementRect: DOMRect;
		relatedElement: ElementType;
		relatedRect: DOMRect;
		placeholderIsInThisRow: boolean;
		horizontalPlaceAfter: boolean;
	}) => void;
	performItemMove?: (argument: { item: T; newIndex: number; oldIndex: number }) => Promise<boolean> | boolean;
	performItemInsert?: (argument: { item: T; newIndex: number }) => Promise<boolean> | boolean;
	performItemRemove?: (argument: { item: T }) => Promise<boolean> | boolean;
};

// External type with some properties optional, as they have defaults:
export type UmbSorterConfig<T, ElementType extends HTMLElement = HTMLElement> = Omit<
	INTERNAL_UmbSorterConfig<T, ElementType>,
	'ignorerSelector' | 'containerSelector'
> &
	Partial<Pick<INTERNAL_UmbSorterConfig<T, ElementType>, 'ignorerSelector' | 'containerSelector'>>;

/**
 * @export
 * @class UmbSorterController
 * @implements {UmbControllerInterface}
 * @description This controller can make user able to sort items.
 */
export class UmbSorterController<T, ElementType extends HTMLElement = HTMLElement> implements UmbController {
	#host;
	#config: INTERNAL_UmbSorterConfig<T, ElementType>;
	#observer;

	#model: Array<T> = [];
	#rqaId?: number;

	#containerElement!: HTMLElement;

	#currentContainerCtrl: UmbSorterController<T, ElementType> = this;
	#currentContainerElement: Element | null = null;
	#useContainerShadowRoot?: boolean;

	#scrollElement?: Element | null;
	#currentElement?: ElementType;
	#currentDragElement?: Element;
	#currentDragRect?: DOMRect;
	#currentItem?: T;

	#currentIndex?: number;
	#dragX = 0;
	#dragY = 0;

	#lastIndicationContainerCtrl: UmbSorterController<T, ElementType> | null = null;

	public get controllerAlias() {
		return this.#config.identifier;
	}

	constructor(host: UmbControllerHostElement, config: UmbSorterConfig<T, ElementType>) {
		this.#host = host;

		// Set defaults:
		config.ignorerSelector ??= 'a, img, iframe';
		if (!config.placeholderClass && !config.placeholderAttr) {
			config.placeholderAttr = 'drag-placeholder';
		}

		this.#config = config as INTERNAL_UmbSorterConfig<T, ElementType>;
		host.addController(this);

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

	setModel(model: Array<T>) {
		if (this.#model) {
			// TODO: Some updates might need to be done, as the modal is about to changed? Do make the changes after setting the model?..
		}
		this.#model = model;
	}

	hostConnected() {
		requestAnimationFrame(this._onFirstRender);
	}
	private _onFirstRender = () => {
		const containerEl =
			(this.#config.containerSelector
				? this.#host.shadowRoot!.querySelector(this.#config.containerSelector)
				: this.#host) ?? this.#host;

		this.#containerElement = containerEl as HTMLElement;
		this.#useContainerShadowRoot = this.#containerElement === this.#host;

		if (!this.#currentContainerElement || this.#currentContainerElement === this.#containerElement) {
			this.#currentContainerElement = containerEl;
		}

		// Only look at the shadowRoot if the containerElement is host.
		const containerElement = this.#useContainerShadowRoot
			? this.#containerElement.shadowRoot ?? this.#containerElement
			: this.#containerElement;
		containerElement.addEventListener('dragover', preventDragOver);

		(this.#containerElement as any)['__umbBlockGridSorterController'] = () => {
			return this;
		};

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
	hostDisconnected() {
		// TODO: Clean up??
		this.#observer.disconnect();
		if (this.#containerElement) {
			(this.#containerElement as any)['__umbBlockGridSorterController'] = undefined;
			this.#containerElement.removeEventListener('dragover', preventDragOver);
			(this.#containerElement as any) = undefined;
		}
	}

	setupItem(element: ElementType) {
		if (this.#config.ignorerSelector) {
			setupIgnorerElements(element, this.#config.ignorerSelector);
		}

		if (!this.#config.disabledItemSelector || !element.matches(this.#config.disabledItemSelector)) {
			element.draggable = true;
			element.addEventListener('dragstart', this.#handleDragStart);
		}

		// If we have a currentItem and the element matches, we should set the currentElement to this element.
		if (this.#currentItem && this.#config.compareElementToModel(element, this.#currentItem)) {
			if (this.#currentElement !== element) {
				console.log('THIS ACTUALLY HAPPENED... NOTICE THIS!');
				this.#setCurrentElement(element);
			}
		}
	}

	destroyItem(element: HTMLElement) {
		if (this.#config.ignorerSelector) {
			destroyIgnorerElements(element, this.#config.ignorerSelector);
		}

		element.removeEventListener('dragstart', this.#handleDragStart);
	}

	#setupPlaceholderStyle() {
		if (this.#config.placeholderClass) {
			this.#currentElement?.classList.add(this.#config.placeholderClass);
		}
		if (this.#config.placeholderAttr) {
			this.#currentElement?.setAttribute(this.#config.placeholderAttr, '');
		}
	}
	#removePlaceholderStyle() {
		if (this.#config.placeholderClass) {
			this.#currentElement?.classList.remove(this.#config.placeholderClass);
		}
		if (this.#config.placeholderAttr) {
			this.#currentElement?.removeAttribute(this.#config.placeholderAttr);
		}
	}

	#setCurrentElement(element: ElementType) {
		this.#currentElement = element;

		this.#currentDragElement = this.#config.draggableSelector
			? element.querySelector(this.#config.draggableSelector) ?? undefined
			: element;

		if (!this.#currentDragElement) {
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

		if (this.#currentElement && this.#currentElement !== element) {
			this.#handleDragEnd();
		}

		event.stopPropagation();
		if (event.dataTransfer) {
			event.dataTransfer.effectAllowed = 'move'; // copyMove when we enhance the drag with clipboard data.
			event.dataTransfer.dropEffect = 'none'; // visual feedback when dropped.
		}

		if (!this.#scrollElement) {
			this.#scrollElement = getParentScrollElement(this.#containerElement, true);
		}

		this.#setCurrentElement(element as ElementType);
		this.#currentDragRect = this.#currentDragElement?.getBoundingClientRect();
		this.#currentItem = this.getItemOfElement(this.#currentElement!);
		if (!this.#currentItem) {
			console.error('Could not find item related to this element.');
			return;
		}

		// Get the current index of the item:
		this.#currentIndex = this.#model.indexOf(this.#currentItem);

		this.#currentElement!.style.transform = 'translateZ(0)'; // Solves problem with FireFox and ShadowDom in the drag-image.

		if (this.#config.dataTransferResolver) {
			this.#config.dataTransferResolver(event.dataTransfer, this.#currentItem);
		}

		if (this.#config.onStart) {
			this.#config.onStart({ item: this.#currentItem, element: this.#currentElement! });
		}

		window.addEventListener('dragover', this.#handleDragMove);
		window.addEventListener('dragend', this.#handleDragEnd);

		// We must wait one frame before changing the look of the block.
		this.#rqaId = requestAnimationFrame(() => {
			// It should be okay to use the same rqaId, as the move does not, or is okay not, to happen on first frame/drag-move.
			this.#rqaId = undefined;
			if (this.#currentElement) {
				this.#currentElement.style.transform = '';
				this.#setupPlaceholderStyle();
			}
		});
	};

	#handleDragEnd = async () => {
		window.removeEventListener('dragover', this.#handleDragMove);
		window.removeEventListener('dragend', this.#handleDragEnd);

		if (!this.#currentElement || !this.#currentItem) {
			return;
		}

		this.#currentElement.style.transform = '';
		this.#removePlaceholderStyle();

		this.#stopAutoScroll();
		this.removeAllowIndication();

		if (this.#config.onEnd) {
			this.#config.onEnd({ item: this.#currentItem, element: this.#currentElement });
		}

		if (this.#rqaId) {
			cancelAnimationFrame(this.#rqaId);
			this.#rqaId = undefined;
		}

		this.#currentContainerElement = this.#containerElement;
		this.#currentContainerCtrl = this;

		this.#currentItem = undefined;
		this.#currentElement = undefined;
		this.#currentDragElement = undefined;
		this.#currentDragRect = undefined;
		this.#dragX = 0;
		this.#dragY = 0;
	};

	#handleDragMove = (event: DragEvent) => {
		if (!this.#currentElement) {
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

			this.#currentDragRect = this.#currentDragElement!.getBoundingClientRect();
			const insideCurrentRect = isWithinRect(this.#dragX, this.#dragY, this.#currentDragRect);
			if (!insideCurrentRect) {
				if (this.#rqaId === undefined) {
					this.#rqaId = requestAnimationFrame(this.#updateDragMove);
				}
			}
		}
	};

	#updateDragMove = () => {
		this.#rqaId = undefined;
		if (!this.#currentElement || !this.#currentContainerElement || !this.#currentItem) {
			return;
		}

		const currentElementRect = this.#currentElement.getBoundingClientRect();
		const insideCurrentRect = isWithinRect(this.#dragX, this.#dragY, currentElementRect);
		if (insideCurrentRect) {
			return;
		}

		let toBeCurrentContainerCtrl: UmbSorterController<T, ElementType> | undefined = undefined;

		// If we have a boundarySelector, try it. If we didn't get anything fall back to currentContainerElement:
		const currentBoundaryElement =
			(this.#config.boundarySelector
				? this.#currentContainerElement.closest(this.#config.boundarySelector)
				: this.#currentContainerElement) ?? this.#currentContainerElement;

		const currentBoundaryRect = currentBoundaryElement.getBoundingClientRect();

		const currentContainerHasItems = this.#currentContainerCtrl.hasOtherItemsThan(this.#currentItem);

		// if empty we will be move likely to accept an item (add 20px to the bounding box)
		// If we have items we must be 10px within the container to accept the move.
		const offsetEdge = currentContainerHasItems ? -10 : 20;
		if (!isWithinRect(this.#dragX, this.#dragY, currentBoundaryRect, offsetEdge)) {
			// we are outside the current container boundary, so lets see if there is a parent we can move to.

			const parentNode = this.#currentContainerElement.parentNode;
			if (parentNode && this.#config.containerSelector) {
				// TODO: support multiple parent shadowDOMs?
				const parentContainer = (parentNode as ShadowRoot).host
					? (parentNode as ShadowRoot).host.closest(this.#config.containerSelector)
					: (parentNode as HTMLElement).closest(this.#config.containerSelector);
				if (parentContainer) {
					const parentContainerCtrl = (parentContainer as any)['__umbBlockGridSorterController']();
					if (parentContainerCtrl.unique === this.controllerAlias) {
						this.#currentContainerElement = parentContainer as Element;
						toBeCurrentContainerCtrl = parentContainerCtrl;
						if (this.#config.onContainerChange) {
							this.#config.onContainerChange({
								item: this.#currentItem,
								element: this.#currentElement,
								//ownerVM: this.#currentContainerVM.ownerVM,
							});
						}
					}
				}
			}
		}

		const containerElement = this.#useContainerShadowRoot
			? this.#currentContainerElement.shadowRoot ?? this.#currentContainerElement
			: this.#currentContainerElement;

		// We want to retrieve the children of the container, every time to ensure we got the right order and index
		const orderedContainerElements = Array.from(containerElement.querySelectorAll(this.#config.itemSelector));

		const currentContainerRect = this.#currentContainerElement.getBoundingClientRect();

		// gather elements on the same row.
		const elementsInSameRow = [];
		let placeholderIsInThisRow = false;
		for (const el of orderedContainerElements) {
			const elRect = el.getBoundingClientRect();
			// gather elements on the same row.
			if (this.#dragY >= elRect.top && this.#dragY <= elRect.bottom) {
				const dragElement = this.#config.draggableSelector ? el.querySelector(this.#config.draggableSelector) : el;
				if (dragElement) {
					const dragElementRect = dragElement.getBoundingClientRect();
					if (el !== this.#currentElement) {
						elementsInSameRow.push({ el: el, dragRect: dragElementRect });
					} else {
						placeholderIsInThisRow = true;
					}
				}
			}
		}

		let lastDistance = Infinity;
		let foundEl: Element | null = null;
		let foundElDragRect!: DOMRect;
		let placeAfter = false;
		elementsInSameRow.forEach((sameRow) => {
			const centerX = sameRow.dragRect.left + sameRow.dragRect.width * 0.5;
			const distance = Math.abs(this.#dragX - centerX);
			if (distance < lastDistance) {
				foundEl = sameRow.el;
				foundElDragRect = sameRow.dragRect;
				lastDistance = distance;
				placeAfter = this.#dragX > centerX;
			}
		});

		if (foundEl) {
			// If we are on top or closest to our self, we should not do anything.
			if (foundEl === this.#currentElement) {
				return;
			}
			const isInsideFound = isWithinRect(this.#dragX, this.#dragY, foundElDragRect, 0);

			// If we are inside the found element, lets look for sub containers.
			// use the itemHasNestedContainersResolver, if not configured fallback to looking for the existence of a container via DOM.
			// TODO: Ability to look into shadowDOMs for sub containers?
			if (
				isInsideFound && this.#config.itemHasNestedContainersResolver
					? this.#config.itemHasNestedContainersResolver(foundEl)
					: (foundEl as HTMLElement).querySelector(this.#config.containerSelector)
			) {
				// Find all sub containers:
				const subLayouts = (foundEl as HTMLElement).querySelectorAll(this.#config.containerSelector);
				for (const subLayoutEl of subLayouts) {
					// Use boundary element or fallback to container element.
					const subBoundaryElement =
						(this.#config.boundarySelector ? subLayoutEl.closest(this.#config.boundarySelector) : subLayoutEl) ||
						subLayoutEl;
					const subBoundaryRect = subBoundaryElement.getBoundingClientRect();

					const subContainerHasItems = subLayoutEl.querySelector(
						this.#config.itemSelector + ':not(.' + this.#config.placeholderClass + ')',
					);
					// gather elements on the same row.
					const subOffsetEdge = subContainerHasItems ? -10 : 20;
					if (isWithinRect(this.#dragX, this.#dragY, subBoundaryRect, subOffsetEdge)) {
						const subCtrl = (subLayoutEl as any)['__umbBlockGridSorterController']();
						if (subCtrl.unique === this.controllerAlias) {
							this.#currentContainerElement = subLayoutEl as HTMLElement;
							toBeCurrentContainerCtrl = subCtrl;
							if (this.#config.onContainerChange) {
								this.#config.onContainerChange({
									item: this.#currentItem,
									element: this.#currentElement,
									//ownerVM: this.#currentContainerVM.ownerVM,
								});
							}
							this.#updateDragMove();
							return;
						}
					}
				}
			}

			// Indication if drop is good:
			if (
				this.updateAllowIndication(toBeCurrentContainerCtrl ?? this.#currentContainerCtrl, this.#currentItem) === false
			) {
				return;
			}

			const verticalDirection = this.#config.resolveVerticalDirection
				? this.#config.resolveVerticalDirection({
						containerElement: this.#currentContainerElement,
						containerRect: currentContainerRect,
						item: this.#currentItem,
						element: this.#currentElement,
						elementRect: currentElementRect,
						relatedElement: foundEl,
						relatedRect: foundElDragRect,
						placeholderIsInThisRow: placeholderIsInThisRow,
						horizontalPlaceAfter: placeAfter,
				  })
				: true;

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
			this.#moveElementTo(toBeCurrentContainerCtrl, newIndex);

			return;
		}
		// We skipped the above part cause we are above or below container:

		// Indication if drop is good:
		if (
			this.updateAllowIndication(toBeCurrentContainerCtrl ?? this.#currentContainerCtrl, this.#currentItem) === false
		) {
			return;
		}

		if (this.#dragY < currentContainerRect.top) {
			this.#moveElementTo(toBeCurrentContainerCtrl, 0);
		} else if (this.#dragY > currentContainerRect.bottom) {
			this.#moveElementTo(toBeCurrentContainerCtrl, -1);
		}
	};

	async #moveElementTo(containerCtrl: UmbSorterController<T, ElementType> | undefined, newIndex: number) {
		if (!this.#currentElement) {
			return;
		}

		containerCtrl ??= this as UmbSorterController<T, ElementType>;

		// If same container and same index, do nothing:
		if (this.#currentContainerCtrl === containerCtrl && this.#currentIndex === newIndex) return;

		if (await containerCtrl.moveItemInModel(newIndex, this.#currentElement, this.#currentContainerCtrl)) {
			this.#currentContainerCtrl = containerCtrl;
			this.#currentIndex = newIndex;
		}
	}

	/** Management methods: */

	public getItemOfElement(element: ElementType) {
		if (!element) {
			return undefined;
		}
		return this.#model.find((entry: T) => this.#config.compareElementToModel(element, entry));
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

	public hasOtherItemsThan(item: T) {
		return this.#model.filter((x) => x !== item).length > 0;
	}

	public async moveItemInModel(newIndex: number, element: ElementType, fromCtrl: UmbSorterController<T, ElementType>) {
		const item = fromCtrl.getItemOfElement(element);
		if (!item) {
			console.error('Could not find item of sync item');
			return false;
		}
		if (this.notifyRequestDrop({ item }) === false) {
			return false;
		}

		const localMove = fromCtrl === this;

		if (localMove) {
			// Local move:

			// TODO: Maybe this should be replaceable/configurable:
			const oldIndex = this.#model.indexOf(item);

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
		} else {
			// Not a local move:

			if ((await fromCtrl.removeItem(item)) !== true) {
				console.error('Sync could not remove item');
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
		}

		return true;
	}

	updateAllowIndication(controller: UmbSorterController<T, ElementType>, item: T) {
		// Remove old indication:
		if (this.#lastIndicationContainerCtrl !== null && this.#lastIndicationContainerCtrl !== controller) {
			this.#lastIndicationContainerCtrl.notifyAllowed();
		}
		this.#lastIndicationContainerCtrl = controller;

		if (controller.notifyRequestDrop({ item: item }) === true) {
			controller.notifyAllowed();
			return true;
		}

		controller.notifyDisallowed(); // This block is not accepted to we will indicate that its not allowed.
		return false;
	}
	removeAllowIndication() {
		// Remove old indication:
		if (this.#lastIndicationContainerCtrl !== null) {
			this.#lastIndicationContainerCtrl.notifyAllowed();
		}
		this.#lastIndicationContainerCtrl = null;
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
			this.#config.onDisallowed();
		}
	}
	public notifyAllowed() {
		if (this.#config.onAllowed) {
			this.#config.onAllowed();
		}
	}
	public notifyRequestDrop(data: any) {
		if (this.#config.onRequestDrop) {
			return this.#config.onRequestDrop(data) || false;
		}
		return true;
	}

	destroy() {
		// Do something when host element is destroyed.
		if (this.#currentElement) {
			this.#handleDragEnd();
		}

		this.#lastIndicationContainerCtrl = null;

		// TODO: Clean up items??
		this.#observer.disconnect();

		// For auto scroller:
		this.#scrollElement = null;
	}
}
