import { Observable, ReplaySubject } from 'rxjs';

export interface UmbRoute {
  path: string;
  elementName: string;
  meta?: any;
}

export interface UmbRouteLocation {
  pathname: string;
  params: object;
  fullPath: string;
  route: UmbRoute;
}

export class UmbRouter {
  private _routes: Array<UmbRoute> = [];
  private _host: HTMLElement;
  private _outlet: HTMLElement;
  private _element: any;

  private _location: ReplaySubject<UmbRouteLocation> = new ReplaySubject(1);
  public readonly location: Observable<UmbRouteLocation> = this._location.asObservable();

  constructor(host: HTMLElement, outlet: HTMLElement) {
    this._host = host;
    this._outlet = outlet;

    // Anchor Hijacker
    this._host.addEventListener('click', async (event: any) => {
      const target = event.composedPath()[0];
      const href = target.href;
      if (!href) return;
      event.preventDefault();

      const url = new URL(href);
      const pathname = url.pathname;

      const canLeave = await this._requestLeave();
      if (!canLeave) return;

      this._navigate(pathname);
    });
  }

  public setRoutes(routes: Array<UmbRoute>) {
    this._routes = routes;
    const pathname = window.location.pathname;
    this.push(pathname);
  }

  public getRoutes() {
    return this._routes;
  }

  public go(delta: number) {
    history.go(delta);
  }

  public back() {
    history.back();
  }

  public forward() {
    history.forward();
  }

  public push(pathname: string) {
    history.pushState(null, '', pathname);
    this._navigate(pathname);
  }

  private async _requestLeave() {
    if (this._element.beforeLeave) {
      const res = await this._element.beforeLeave();
      if (!res) return;
    }

    const beforeLeaveEvent = new CustomEvent('before-leave', {
      bubbles: true,
      composed: true,
      cancelable: true,
    });

    this._host.dispatchEvent(beforeLeaveEvent);

    if (beforeLeaveEvent.defaultPrevented) return;

    return true;
  }

  private async _requestEnter(to: UmbRouteLocation) {
    if (this._element.beforeEnter) {
      const res = await this._element.beforeEnter();
      if (!res) return;
    }

    const beforeEnterEvent = new CustomEvent('before-enter', {
      bubbles: true,
      composed: true,
      cancelable: true,
      detail: { to },
    });

    this._host.dispatchEvent(beforeEnterEvent);

    if (beforeEnterEvent.defaultPrevented) return;

    return true;
  }

  private async _navigate(pathname: string) {
    const location = this._resolve(pathname);
    if (!location) return;

    this._setupElement(location);

    const canEnter = await this._requestEnter(location);
    if (!canEnter) return;

    window.history.pushState(null, '', pathname);

    this._location.next(location);
    this._render();
  }

  private _resolve(pathname: string): UmbRouteLocation | null {
    let location: UmbRouteLocation | null = null;

    this._routes.forEach((route) => {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      const pattern = new URLPattern({ pathname: route.path });
      const href = `${window.location.origin}${pathname}`;
      const match = pattern.test(href);

      if (match) {
        const result = pattern.exec(href);
        location = {
          pathname: result.pathname.input,
          params: result.pathname.groups,
          fullPath: result.pathname.input,
          route,
        };
      }
    });

    return location;
  }

  private _setupElement(location: UmbRouteLocation) {
    this._element = document.createElement<any>(location.route.elementName);
    this._element.location = location;
  }

  private async _render() {
    const childNodes = this._outlet.childNodes;
    childNodes.forEach((node) => {
      this._outlet.removeChild(node);
    });

    this._outlet.appendChild(this._element);
  }
}
