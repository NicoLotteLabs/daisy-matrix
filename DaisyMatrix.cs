using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Diagnostics;

static class Native {
 [StructLayout(LayoutKind.Sequential)] public struct Mem {public uint Length,Load;public ulong TotalPhys,AvailPhys,TotalPage,AvailPage,TotalVirtual,AvailVirtual,AvailExt;}
 [DllImport("kernel32",SetLastError=true)] public static extern bool GlobalMemoryStatusEx(ref Mem m);
 [DllImport("kernel32",SetLastError=true)] public static extern bool GetSystemTimes(out long idle,out long kernel,out long user);
 [StructLayout(LayoutKind.Sequential)] public struct Coord { public short X,Y; public Coord(short x,short y){X=x;Y=y;} }
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int L,T,R,B; }
 [StructLayout(LayoutKind.Sequential)] public struct SmallRect { public short L,T,R,B; }
 [StructLayout(LayoutKind.Sequential)] public struct Info { public Coord Size, Cursor; public ushort Attr; public SmallRect Window; public Coord Max; }
 [DllImport("kernel32",SetLastError=true)] public static extern bool AttachConsole(uint pid);
 [DllImport("kernel32")] public static extern bool FreeConsole();
 [DllImport("kernel32")] public static extern IntPtr GetConsoleWindow();
 [DllImport("kernel32",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr CreateFile(string name,uint access,uint share,IntPtr sec,uint mode,uint flags,IntPtr template);
 [DllImport("kernel32")] public static extern bool GetConsoleScreenBufferInfo(IntPtr h,out Info info);
 [DllImport("kernel32",CharSet=CharSet.Unicode)] public static extern bool ReadConsoleOutputCharacter(IntPtr h,StringBuilder buf,uint len,Coord pos,out uint read);
 [DllImport("kernel32")] public static extern bool SetConsoleTextAttribute(IntPtr h,ushort attr);
 [DllImport("kernel32")] public static extern bool CloseHandle(IntPtr h);
 [DllImport("user32")] public static extern short GetAsyncKeyState(int key);
 [DllImport("user32")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32")] public static extern bool GetClientRect(IntPtr h,out Rect r);
 [DllImport("user32")] public static extern bool ClientToScreen(IntPtr h,ref Point p);
 [DllImport("user32")] public static extern bool IsIconic(IntPtr h);
 [DllImport("user32")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
}
class Rain : Form {
 readonly Random rng=new Random();
 int[] heads,lengths,speeds; char[,] chars; int tick; 
 const string Glyphs="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ#$%&*+-<>:;@";
 readonly Font glyph=new Font("Consolas",13,FontStyle.Regular,GraphicsUnit.Pixel);
 readonly Font title=new Font("Consolas",23,FontStyle.Bold,GraphicsUnit.Pixel);
 readonly Font sub=new Font("Consolas",13,FontStyle.Regular,GraphicsUnit.Pixel);
 readonly Font big=new Font("Consolas",22,FontStyle.Bold,GraphicsUnit.Pixel);
 readonly Font small=new Font("Consolas",12,FontStyle.Regular,GraphicsUnit.Pixel);
 readonly SolidBrush[] greens=new SolidBrush[20];
 readonly SolidBrush head=new SolidBrush(Color.FromArgb(205,255,218));
 long oldIdle,oldKernel,oldUser; bool haveCpu; float cpuVal=-1,freeMb=-1; double disk=-1; 
 string elapsed="00:00:00", machine="LOCAL", lastLog="--:--:--";
 DateTime sample=DateTime.MinValue;
 int cols,rows; bool initialized;
 public bool FullScreenMode {get;private set;}
 Rectangle FullButton {get{return new Rectangle(Math.Max(4,Width-164),28,152,24);}}
 public void ToggleFullScreen(){FullScreenMode=!FullScreenMode;Invalidate();}
 public Rain(){FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false;TopMost=true;BackColor=Color.Black;DoubleBuffered=true;
  for(int i=0;i<20;i++)greens[i]=new SolidBrush(Color.FromArgb(0,20+i*10,7+i*2));
  haveCpu=Native.GetSystemTimes(out oldIdle,out oldKernel,out oldUser);
 }
 protected override bool ShowWithoutActivation {get{return true;}}
 protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x08000000|0x80;return p;}}
 protected override void WndProc(ref Message m){
  if(m.Msg==0x21){m.Result=new IntPtr(3);return;}
  if(m.Msg==0x84){long v=m.LParam.ToInt64();Point pt=PointToClient(new Point((short)(v&65535),(short)((v>>16)&65535)));m.Result=new IntPtr(FullButton.Contains(pt)?1:-1);return;}
  base.WndProc(ref m);
 }
 protected override void OnMouseUp(MouseEventArgs e){if(e.Button==MouseButtons.Left&&FullButton.Contains(e.Location))ToggleFullScreen();base.OnMouseUp(e);}
 void Sample(){
  if((DateTime.UtcNow-sample).TotalSeconds<3)return;sample=DateTime.UtcNow;
  long i,k,u;if(Native.GetSystemTimes(out i,out k,out u)){long total=k-oldKernel+u-oldUser;if(haveCpu&&total>0)cpuVal=(float)Math.Max(0,Math.Min(100,100.0*(total-(i-oldIdle))/total));oldIdle=i;oldKernel=k;oldUser=u;haveCpu=true;}
  var m=new Native.Mem();m.Length=(uint)Marshal.SizeOf(typeof(Native.Mem));if(Native.GlobalMemoryStatusEx(ref m))freeMb=(float)(m.AvailPhys/1048576.0);
  try{var drive=new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));disk=100.0*(drive.TotalSize-drive.AvailableFreeSpace)/drive.TotalSize;}catch{disk=-1;}
 }
 public void Step(DateTime changed){tick++;elapsed=TimeSpan.FromSeconds(Math.Max(0,(DateTime.UtcNow-changed).TotalSeconds)).ToString(@"hh\:mm\:ss");lastLog=changed.ToLocalTime().ToString("HH:mm:ss");Sample();Invalidate();}
 void T(Graphics g,string s,Font f,int x,int y,Color c){using(var brush=new SolidBrush(c))g.DrawString(s,f,brush,x,y);}
 void Bar(Graphics g,int x,int y,int w,double val){if(w<1)return;using(var b=new SolidBrush(Color.FromArgb(4,42,19)))g.FillRectangle(b,x,y,w,5);if(val>=0)using(var b=new SolidBrush(Color.FromArgb(49,205,96)))g.FillRectangle(b,x,y,(int)(w*Math.Min(100,val)/100),5);}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.Clear(Color.FromArgb(0,7,2));int n=Math.Max(1,Width/15),r=Math.Max(1,(Height-72)/17);
  if(!initialized||cols!=n||rows!=r){initialized=true;cols=n;rows=r;heads=new int[n];lengths=new int[n];speeds=new int[n];chars=new char[n,r];for(int i=0;i<n;i++){heads[i]=rng.Next(-r,r);lengths[i]=rng.Next(6,25);speeds[i]=rng.Next(1,5);for(int j=0;j<r;j++)chars[i,j]=Glyphs[rng.Next(Glyphs.Length)];}}
  for(int i=0;i<n;i++){if((tick+i)%speeds[i]==0){heads[i]++;if(heads[i]>=0&&heads[i]<r)chars[i,heads[i]]=Glyphs[rng.Next(Glyphs.Length)];}if(heads[i]-lengths[i]>r){heads[i]=-rng.Next(1,r+1);lengths[i]=rng.Next(6,25);}for(int j=0;j<lengths[i];j++){int row=heads[i]-j;if(row<0||row>=r)continue;g.DrawString(chars[i,row].ToString(),glyph,j==0?head:greens[Math.Max(0,19-j*19/lengths[i])],i*15,58+row*17);}}
  Color bright=Color.FromArgb(102,255,143),muted=Color.FromArgb(67,169,102);
  using(var b=new SolidBrush(Color.FromArgb(0,16,7)))g.FillRectangle(b,0,0,Width,57);
  using(var p=new Pen(Color.FromArgb(0,145,55)))g.DrawLine(p,0,56,Width,56);
  T(g,"DAISY // MATRIX",title,15,5,bright);T(g,"NICOLOTTE LABS",small,17,34,muted);
  string clock=DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");if(Width>480)T(g,clock,sub,Width-190,5,bright);
  using(var b=new SolidBrush(Color.FromArgb(8,45,20)))g.FillRectangle(b,FullButton);
  using(var p=new Pen(Color.FromArgb(0,145,55)))g.DrawRectangle(p,FullButton);
  T(g,FullScreenMode?"F11  WINDOW":"F11  FULL SCREEN",small,FullButton.X+7,FullButton.Y+5,bright);
  int panel=Math.Min(310,Math.Max(200,Width/3)),x=Width-panel-18,y=77;
  if(Width>580&&Height>300){
   using(var b=new SolidBrush(Color.FromArgb(235,0,16,7)))g.FillRectangle(b,x,y,panel,217);
   using(var p=new Pen(Color.FromArgb(0,120,49)))g.DrawRectangle(p,x,y,panel,217);
   T(g,"SYSTEME / "+machine,sub,x+12,y+12,bright);
   T(g,"CPU TOTAL",small,x+12,y+42,muted);T(g,cpuVal<0?"N/D":cpuVal.ToString("0")+" %",big,x+12,y+56,bright);Bar(g,x+12,y+85,panel-24,cpuVal);
   T(g,"RAM DISPONIBLE",small,x+12,y+101,muted);T(g,freeMb<0?"N/D":(freeMb/1024).ToString("0.0")+" Go",big,x+12,y+115,bright);
   T(g,"DISQUE SYSTEME UTILISE",small,x+12,y+148,muted);T(g,disk<0?"N/D":disk.ToString("0")+" %",big,x+12,y+162,bright);Bar(g,x+12,y+195,panel-24,disk);
  }
  int bw=Math.Min(460,Width-36),bx=18,by=Math.Max(65,Height-111);
  if(bw>100&&Height>170){
   using(var b=new SolidBrush(Color.FromArgb(240,0,17,7)))g.FillRectangle(b,bx,by,bw,78);
   using(var p=new Pen(Color.FromArgb(0,120,49)))g.DrawRectangle(p,bx,by,bw,78);
   T(g,"VEILLE VISUELLE  /  CONSOLE SILENCIEUSE",sub,bx+10,by+7,bright);
   T(g,"Silence : "+elapsed+"   Dernier log : "+lastLog,small,bx+10,by+31,muted);
   T(g,"ESC : afficher les logs",small,bx+10,by+52,muted);
  }
  using(var b=new SolidBrush(Color.FromArgb(0,15,6)))g.FillRectangle(b,0,Height-24,Width,24);
  T(g,"DAISY MATRIX v1.0.0  |  MIT  |  support@nicolottelabs.com",small,12,Height-20,muted);
 }
}
static class Program {
 static string status=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"matrix-status.txt");
 static void State(string s){try{File.WriteAllText(status,DateTime.Now.ToString("s")+" "+s);}catch{}}
 [STAThread] static void Main(string[] args){
  if(args.Length<1)return;uint pid;if(!uint.TryParse(args[0],out pid))return;
  bool first;using(var mutex=new System.Threading.Mutex(true,"Local\\DaisyMatrixPublic",out first)){if(!first)return;
  Native.FreeConsole();if(!Native.AttachConsole(pid)){State("ATTACH_FAILED "+Marshal.GetLastWin32Error());return;}
  var h=Native.CreateFile("CONOUT$",0xC0000000,3,IntPtr.Zero,3,0,IntPtr.Zero);var win=Native.GetConsoleWindow();Native.Info info;
  if(h==new IntPtr(-1)||win==IntPtr.Zero||!Native.GetConsoleScreenBufferInfo(h,out info)){State("CONSOLE_FAILED");return;}
  ushort original=info.Attr;bool green=Native.SetConsoleTextAttribute(h,10);State("MONITORING color="+green);
  Application.EnableVisualStyles();var form=new Rain();var timer=new Timer();timer.Interval=125;
  string last=null, state="";DateTime changed=DateTime.UtcNow;DateTime suppressUntil=DateTime.MinValue;int cycles=0;bool f11Down=false;
  timer.Tick+=(s,e)=>{
   try{
    if(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"stop.request"))){Application.ExitThread();return;} using(var target=Process.GetProcessById((int)pid)){if(target.HasExited){Application.ExitThread();return;}}
    if(++cycles%4==1){
     Native.Info ci;if(!Native.GetConsoleScreenBufferInfo(h,out ci)){Application.ExitThread();return;}
     int start=Math.Max(0,ci.Cursor.Y-5), count=Math.Max(1,ci.Size.X*(ci.Cursor.Y-start+1));
     var buf=new StringBuilder(count+1);uint read;
     if(Native.ReadConsoleOutputCharacter(h,buf,(uint)count,new Native.Coord(0,(short)start),out read)){
      string sample=ci.Cursor.X+":"+ci.Cursor.Y+":"+buf.ToString().GetHashCode();
      if(sample!=last){last=sample;changed=DateTime.UtcNow;}
     }else changed=DateTime.UtcNow;
    }
    bool idle=(DateTime.UtcNow-changed).TotalSeconds>=10;
    bool foreground=Native.GetForegroundWindow()==win&&!Native.IsIconic(win);
    if(foreground && form.Visible && (Native.GetAsyncKeyState(27)&0x8000)!=0) suppressUntil=DateTime.UtcNow.AddSeconds(30);
    bool f11=(Native.GetAsyncKeyState(122)&0x8000)!=0;
    if(foreground&&form.Visible&&f11&&!f11Down)form.ToggleFullScreen();f11Down=f11;
    bool allowed=DateTime.UtcNow>=suppressUntil;
    if(idle&&foreground&&allowed){
     Native.Rect r;Point p=new Point(0,0);
     if(Native.GetClientRect(win,out r)&&Native.ClientToScreen(win,ref p)&&r.R>100&&r.B>100){
      if(!form.Visible)form.Show();
      Rectangle bounds=form.FullScreenMode?Screen.FromHandle(win).Bounds:new Rectangle(p.X,p.Y,r.R,r.B);
      Native.SetWindowPos(form.Handle,new IntPtr(-1),bounds.X,bounds.Y,bounds.Width,bounds.Height,0x0010|0x0040);
      form.Step(changed);
     }
    }else if(form.Visible)form.Hide();
    string next=idle?(foreground?(allowed?"MATRIX_VISIBLE":"MANUAL_LOGS"):"IDLE_CONSOLE_NOT_FOREGROUND"):"ACTIVE_LOGS";
    next=next+(form.FullScreenMode?" FULLSCREEN":" WINDOW");if(next!=state){state=next;State(state+" color="+green);}
   }catch(ArgumentException){Application.ExitThread();}
  };
  Application.ApplicationExit+=(s,e)=>{timer.Stop();Native.SetConsoleTextAttribute(h,original);Native.CloseHandle(h);Native.FreeConsole();State("STOPPED");};
  timer.Start();Application.Run();
 }}
}