import { Observable, ReplaySubject } from 'rxjs';

import { UmbRouterBeforeEnterEvent } from './router-before-enter.event';
import { UmbRouterBeforeLeaveEvent } from './router-before-leave.event';

export interface UmbRoute {
  path: string;
  alias: string;
  meta?: any;
}

export interface UmbRouteLocation {
  pathname: string;
  params: Record<string, any>;
  fullPath: string;
  route: UmbRoute;
}

export interface UmbRouteElement extends HTMLElement {
  location?: UmbRouteLocation;
  beforeEnter?: (to: UmbRouteLocation) => Promise<boolean>;
  beforeLeave?: (to: UmbRouteLocation) => Promise<boolean>;
}

export class UmbRouter {
  private _routes: Array<UmbRoute> = [];
  private _host: HTMLElement;
  private _element?: UmbRouteElement;

  private _location: ReplaySubject<UmbRouteLocation> = new ReplaySubject(1);
  public readonly location: Observable<UmbRouteLocation> = this._location.asObservable();

  constructor(host: HTMLElement) {
    this._host = host;

    // Anchor Hijacker
    this._host.addEventListener('click', async (event: any) => {
      const target = event.composedPath()[0];
      const href = target.href;
      if (!href) return;
      event.preventDefault();

      const url = new URL(href);
      const pathname = url.pathname;

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

  private async _requestLeave(to: UmbRouteLocation) {
    if (typeof this._element?.beforeLeave === 'function') {
      const res = await this._element.beforeLeave(to);
      if (!res) return;
    }

    const beforeLeaveEvent = new UmbRouterBeforeLeaveEvent(to);
    this._host.dispatchEvent(beforeLeaveEvent);

    if (beforeLeaveEvent.defaultPrevented) return;

    return true;
  }

  private async _requestEnter(to: UmbRouteLocation) {
    if (typeof this._element?.beforeEnter === 'function') {
      const res = await this._element.beforeEnter(to);
      if (!res) return;
    }

    const beforeEnterEvent = new UmbRouterBeforeEnterEvent(to);
    this._host.dispatchEvent(beforeEnterEvent);

    if (beforeEnterEvent.defaultPrevented) return;

    return true;
  }

  private async _navigate(pathname: string) {
    const location = this._resolve(pathname);
    if (!location) return;

    const canLeave = await this._requestLeave(location);
    if (!canLeave) return;

    const canEnter = await this._requestEnter(location);
    if (!canEnter) return;

    window.history.pushState(null, '', pathname);

    this._location.next(location);
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
}
