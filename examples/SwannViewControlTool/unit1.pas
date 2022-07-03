{ Skype Audio Manager (C) Copyright 2018 by James O. Dreher

  Right-click on the icon in the system tray to select audio device.
  For Skype for Business on Window 10

  This is free and unencumbered software released into the public domain.

  Anyone is free to copy, modify, publish, use, compile, sell, or
  distribute this software, either in source code form or as a compiled
  binary, for any purpose, commercial or non-commercial, and by any
  means.

  In jurisdictions that recognize copyright laws, the author or authors
  of this software dedicate any and all copyright interest in the
  software to the public domain. We make this dedication for the benefit
  of the public at large and to the detriment of our heirs and
  successors. We intend this dedication to be an overt act of
  relinquishment in perpetuity of all present and future rights to this
  software under copyright law.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
  IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
  OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
  ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
  OTHER DEALINGS IN THE SOFTWARE.

  For more information, please refer to <http://unlicense.org>
}

unit Unit1;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, Forms, Controls, Dialogs, StdCtrls,
  ExtCtrls, Menus, Windows, ShellApi, WinAutoKey, DebugUnit;

type

  { TForm1 }

  TForm1 = class(TForm)
    ButtonOK: TButton;
    Memo1: TMemo;
    miStartExe: TMenuItem;
    miShow: TMenuItem;
    miExit: TMenuItem;
    PopupMenu1: TPopupMenu;
    TrayIcon: TTrayIcon;
    procedure StartAsync(Data: PtrInt);
    procedure FormShow(Sender: TObject);
    function StartExePrompt: Boolean;
    procedure ButtonOKClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var CloseAction: TCloseAction);
    procedure miExitClick(Sender: TObject);
    procedure miShowClick(Sender: TObject);
    procedure miStartExeClick(Sender: TObject);
    procedure TrayIconClick(Sender: TObject);
  private

  public

  end;
const
  DS=DirectorySeparator;
  LE=LineEnding;

  LOGIN_WINDOW_TITLE='Search & Login';
  MYDVR_WINDOW_TITLE='SwannView Link';
  EXE_NAME='"C:\Program Files (x86)\SwannView Link\MyDVR.exe"';

var
  Form1: TForm1;

implementation

{$R *.lfm}

{ TForm1 }

procedure TForm1.StartAsync(Data: PtrInt);
begin
  //StartExePrompt;
end;

procedure TForm1.ButtonOKClick(Sender: TObject);
begin
  Form1.Hide;
end;

function TForm1.StartExePrompt: Boolean;
var
  hLogin, hMyDvr: HWND;
  Reply: integer;
begin

  if WinExists(hWin(LOGIN_WINDOW_TITLE)) then Exit;
  if WinExists(hWin(MYDVR_WINDOW_TITLE)) then Exit;

  Reply:=IDYES;

  //Reply:=MsgBox('Start MyDVR?', Application.Title,MB_ICONQUESTION + MB_YESNO);

  if Reply=IDYES then begin

    ShellExecute(Handle, 'open', EXE_NAME, nil, nil, SW_SHOWNORMAL);

    hLogin:=WinWait(LOGIN_WINDOW_TITLE,'','',15);
    if hLogin=0 then begin
      Form1.Show;
      Memo1.Append('Error timeout waiting for window: '+LOGIN_WINDOW_TITLE);
      Exit;
    end;

    if not WinActivate(hLogin) then begin
      Form1.Show;
      Memo1.Append('Error could not activate: '+LOGIN_WINDOW_TITLE);
      Exit;
    end;

    ControlClick(hCtl(hLogin,'Login'),mbLeft,[]);

    hMyDvr:=WinWait(MYDVR_WINDOW_TITLE,'','',15);
    if hMyDvr=0 then begin
      Form1.Show;
      Memo1.Append('Error timeout waiting for window: '+MYDVR_WINDOW_TITLE);
      Exit;
    end;

    if not WinActivate(hMyDvr) then begin
      Form1.Show;
      Memo1.Append('Error could not activate: '+MYDVR_WINDOW_TITLE);
      Exit;
    end;

    //ok MouseClick(mbLeft,[],1600,1060);
    ControlClick(hCtl(hMyDvr,'','',994),mbLeft,[]); //Split
    WinSleep(500);

    { this works, but could find a duplicate class #3270, therefore use the mouse click
    hMyDvr:=WinWait('','','#32770',5);
    if hMyDvr=0 then begin
      Form1.Show;
      Memo1.Append('Error timeout waiting for window: '+MYDVR_WINDOW_TITLE);
      Exit;
    end;

    if not WinActivate(hMyDvr) then begin
      Form1.Show;
      Memo1.Append('Error could not activate: '+MYDVR_WINDOW_TITLE);
      Exit;
    end;

    //ok MouseClick(mbLeft,[],1660,995);
    ControlClick(hCtl(hMyDvr,'4','',5501),mbLeft,[]);  //4-way
}

  MouseClick(mbLeft,[],1660,995);

  Result:=True;

  end else begin
    TrayIcon.Hint:='MyDVR Not Running';
    Result:=False;
  end;
end;

procedure TForm1.miStartExeClick(Sender: TObject);
begin
  if not WinExists(hWin(MYDVR_WINDOW_TITLE)) then begin
    StartExePrompt;
  end;
end;

procedure TForm1.miShowClick(Sender: TObject);
begin
  Form1.Show;
end;

procedure TForm1.miExitClick(Sender: TObject);
begin
  Form1.Close;
end;

procedure TForm1.TrayIconClick(Sender: TObject);
begin
  Form1.Show;
end;

procedure TForm1.FormCreate(Sender: TObject);
begin

  Application.ShowMainForm:=False;

  TrayIcon.Hint:='MyDVR Manager';
  TrayIcon.BalloonTitle:=TrayIcon.Hint;
  TrayIcon.Visible:=True;

  SetTitleMatchMode(mtExact);
  SetTextMatchMode(mtStartsWith);

  SetKeyDelay(10);   //default 5ms
  SetWinDelay(200);   //default 100ms

  Application.QueueAsyncCall(@StartAsync,0);
end;
procedure TForm1.FormShow(Sender: TObject);
begin
  //DebugForm.Show;
  //Debugln('test');
 //StartExePrompt;
end;

procedure TForm1.FormClose(Sender: TObject; var CloseAction: TCloseAction);
var
  hLogin, hMyDvr: HWND;
  Reply: integer;
begin

  hMyDvr:=hWin(MYDVR_WINDOW_TITLE);
  if hMyDvr<>0 then begin
    if not WinActivate(hMyDvr) then begin
      Form1.Show;
      Memo1.Append('Error could not activate: '+MYDVR_WINDOW_TITLE);
      Exit;
    end;
    ControlClick(hCtl(hMyDvr,'','',1015),mbLeft,[]);  //Logout
  end;

  hLogin:=WinWait(LOGIN_WINDOW_TITLE,'','',5);
  if hLogin<>0 then begin
    if not WinActivate(hLogin) then begin
      Form1.Show;
      Memo1.Append('Error could not activate: '+LOGIN_WINDOW_TITLE);
      Exit;
    end;
    ControlClick(hCtl(hLogin,'Cancel'),mbLeft,[]);
  end;

end;
end.

