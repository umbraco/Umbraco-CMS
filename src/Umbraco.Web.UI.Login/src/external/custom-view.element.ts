import {LitElement} from "lit";
import {customElement, property, state} from "lit/decorators.js";
import {until} from "lit/directives/until.js";
import {loadCustomView, renderCustomView} from "../utils/load-custom-view.function.js";

@customElement('umb-custom-view')
export default class UmbCustomViewElement extends LitElement {
  @property({ attribute: 'custom-view' })
  set customView (value: string) {
    this.#customView = value;
    this.#loadView();
  }

  @property({ type: Object, attribute: 'args'})
  set args (value: any) {
    this.#args = value;
    this.#loadView();
  }

  @state()
  protected component: any = null;

  #args?: any;
  #customView?: string;

  async #loadView() {
    if (!this.#customView || !this.#customView.endsWith('.js') && !this.#customView.endsWith('.html')) {
      return;
    }

    const customView = await loadCustomView(this.#customView);

    if (this.#args) {
      Object.entries(this.#args).forEach(([key, value]) => {
        (customView as any)[key] = value;
      });
    }

    this.component = renderCustomView(customView);
  }

  render() {
    return until(this.component);
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-custom-view': UmbCustomViewElement;
  }
}
